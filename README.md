# Ntech-ai

## scripts/train_lora.py

Fine-tuning script for causal language models using PEFT/LoRA.

Usage (example):

python train_lora.py --model_name_or_path <model> --train_file <train.jsonl> --output_dir outputs/lora

Notes:
- train_file should be a JSONL with each line: {"prompt": "…", "completion": "…"}.
- --target_modules: when omitted or empty, the script auto-detects sensible target modules for common models (GPT-2, LLaMA, Bloom). Provide a comma-separated list to override detection.
- The script saves the PEFT adapter and tokenizer to the output directory when finished.

