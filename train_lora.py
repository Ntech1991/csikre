#!/usr/bin/env python3
"""
train_lora.py — HF + PEFT LoRA auto-detect training script (causal LM / SFT).

This script fine-tunes causal LMs using PEFT/LoRA. If --target_modules is left empty,
the script will attempt to auto-detect sensible module names for common model types
(e.g., GPT-2, LLaMA, Bloom). Provide a comma-separated list to override detection.
"""
import argparse
import os
import json
import torch
from transformers import (
    AutoTokenizer,
    AutoModelForCausalLM,
    TrainingArguments,
    Trainer,
    DataCollatorForLanguageModeling,
)
from datasets import load_dataset
from peft import LoraConfig, get_peft_model, prepare_model_for_kbit_training


def parse_args():
    p = argparse.ArgumentParser()
    p.add_argument("--model_name_or_path", required=True)
    p.add_argument("--train_file", required=True)  # JSONL with {"prompt": "...", "completion":"..."}
    p.add_argument("--valid_file", default=None)
    p.add_argument("--output_dir", default="outputs/lora")
    p.add_argument("--per_device_batch_size", type=int, default=8)
    p.add_argument("--gradient_accumulation_steps", type=int, default=4)
    p.add_argument("--learning_rate", type=float, default=2e-4)
    p.add_argument("--num_train_epochs", type=int, default=3)
    p.add_argument("--max_seq_length", type=int, default=1024)
    p.add_argument("--lora_r", type=int, default=16)
    p.add_argument("--lora_alpha", type=int, default=32)
    p.add_argument("--lora_dropout", type=float, default=0.05)
    p.add_argument("--target_modules", type=str, default="", help="Comma-separated. If empty, auto-detects suitable target modules for the model (recommended).")
    return p.parse_args()


def build_prompt(example: dict) -> str:
    # Simple concatenation; adapt to your instruction format
    prompt = example.get("prompt", "")
    completion = example.get("completion", "")
    return prompt + completion


def tokenize_fn(ex, tokenizer, max_length):
    text = build_prompt(ex)
    out = tokenizer(text, truncation=True, max_length=max_length)
    # Use labels identical to input_ids for causal LM fine-tuning
    out["labels"] = out["input_ids"].copy()
    return out


def main():
    args = parse_args()

    use_cuda = torch.cuda.is_available()
    try:
        import bitsandbytes as bnb
        have_bnb = True
    except Exception:
        have_bnb = False

    # device/dtype decisions
    device = "cuda" if use_cuda else "cpu"
    dtype = torch.float16 if use_cuda else torch.float32

    tokenizer = AutoTokenizer.from_pretrained(args.model_name_or_path, use_fast=True)
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token
        # make sure pad_token_id is set too
        if tokenizer.pad_token_id is None and tokenizer.eos_token_id is not None:
            tokenizer.pad_token_id = tokenizer.eos_token_id

    # model loading: prefer 8-bit if GPU+bnb, else normal
    load_kwargs = {}
    if use_cuda and have_bnb:
        load_kwargs.update({"load_in_8bit": True, "device_map": "auto"})
    else:
        load_kwargs.update({"torch_dtype": dtype})

    model = AutoModelForCausalLM.from_pretrained(args.model_name_or_path, **load_kwargs)

    if use_cuda and have_bnb:
        # prepare for k-bit training when using bitsandbytes
        model = prepare_model_for_kbit_training(model)

    # LoRA config: validate or auto-fallback target modules
        requested_target_modules = [t.strip() for t in args.target_modules.split(",") if t.strip()]

        # helper to check presence of substrings in module names
        module_names = [name for name, _ in model.named_modules()]
        def any_module_matches(candidates):
            for cand in candidates:
                for name in module_names:
                    if cand in name:
                        return True
            return False

        if not any_module_matches(requested_target_modules):
            model_type = getattr(getattr(model, "config", None), "model_type", "").lower()
            # sensible fallbacks for common model types
            if "gpt2" in model_type or "gpt" in model_type:
                fallback = ["c_attn", "c_proj"]
            elif "llama" in model_type:
                fallback = ["q_proj", "v_proj"]
            elif "bloom" in model_type:
                fallback = ["query_key_value"]
            else:
                fallback = ["q_proj", "v_proj", "c_attn", "c_proj"]

            # filter fallback to modules that actually appear in the model
            filtered = [f for f in fallback if any(f in n for n in module_names)]
            if filtered:
                print(f"Warning: requested target_modules {requested_target_modules} not found; using detected targets: {filtered}")
                target_modules = filtered
            else:
                # final fallback: use requested targets even if not matched (let PEFT raise a clear error)
                print(f"Warning: could not auto-detect target modules for model type '{model_type}'. Using requested values: {requested_target_modules}")
                target_modules = requested_target_modules
        else:
            target_modules = requested_target_modules

        lora_cfg = LoraConfig(
            r=args.lora_r,
            lora_alpha=args.lora_alpha,
            target_modules=target_modules,
            lora_dropout=args.lora_dropout,
            bias="none",
            task_type="CAUSAL_LM",
        )
        model = get_peft_model(model, lora_cfg)

    # dataset
    data_files = {"train": args.train_file}
    if args.valid_file:
        data_files["validation"] = args.valid_file

    ds = load_dataset("json", data_files=data_files)

    # remember original columns (from the train split) and remove them after tokenization
    orig_columns = ds["train"].column_names

    # tokenization
    tokenized = ds.map(
        lambda ex: tokenize_fn(ex, tokenizer, args.max_seq_length),
        remove_columns=orig_columns,
        batched=False,
    )

    data_collator = DataCollatorForLanguageModeling(tokenizer=tokenizer, mlm=False)

    # Build TrainingArguments with optional eval settings only when a validation file is provided
    ta_kwargs = dict(
        output_dir=args.output_dir,
        per_device_train_batch_size=args.per_device_batch_size,
        gradient_accumulation_steps=args.gradient_accumulation_steps,
        learning_rate=args.learning_rate,
        num_train_epochs=args.num_train_epochs,
        logging_steps=50,
        save_strategy="steps",
        save_steps=500,
        fp16=use_cuda,
        weight_decay=0.01,
        save_total_limit=3,
        push_to_hub=False,
        remove_unused_columns=False,
    )
    if args.valid_file:
        ta_kwargs.update({"eval_strategy": "steps", "eval_steps": 500})

    training_args = TrainingArguments(**ta_kwargs)

    trainer = Trainer(
        model=model,
        args=training_args,
        train_dataset=tokenized["train"],
        eval_dataset=tokenized["validation"] if args.valid_file else None,
        data_collator=data_collator,
    )

    trainer.train()

    # Save the PEFT adapter and tokenizer. For PEFT models, use save_pretrained to persist adapters.
    os.makedirs(args.output_dir, exist_ok=True)
    try:
        # model may be a PeftModel wrapper — use its save_pretrained method
        model.save_pretrained(args.output_dir)
    except Exception:
        # fallback to Trainer.save_model which will handle some cases
        trainer.save_model(args.output_dir)

    tokenizer.save_pretrained(args.output_dir)
    print("Training complete. Model (adapter) and tokenizer saved to", args.output_dir)


if __name__ == "__main__":
    main()
