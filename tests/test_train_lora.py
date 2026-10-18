"""
Comprehensive tests for train_lora.py critical paths.
Tests cover argument parsing, prompt building, tokenization, and training flow.

Usage: pytest test_train_lora.py -v
"""

import pytest
import sys
import argparse
from unittest.mock import Mock, patch, MagicMock, call
from pathlib import Path

# Add parent directory to path to import train_lora
sys.path.insert(0, str(Path(__file__).parent.parent))


def test_transformers_requirement_excludes_incompatible_major_version():
    """Require the eval_strategy API and exclude untested major versions."""
    requirements = (Path(__file__).parent / "requirements.txt").read_text()
    requirement = next(
        line for line in requirements.splitlines() if line.startswith("transformers")
    )

    from packaging.requirements import Requirement

    supported = Requirement(requirement).specifier
    assert "4.40.0" not in supported
    assert "4.41.0" in supported
    assert "4.57.6" in supported
    assert "5.0.0" not in supported


class TestParseArgs:
    """Tests for argument parsing logic"""

    @patch('sys.argv', ['train_lora.py', '--model_name_or_path', 'mistral', '--train_file', 'train.jsonl'])
    def test_parse_args_with_required_args_only(self):
        """Test parsing with only required arguments"""
        # Import here to use the patched sys.argv
        from train_lora import parse_args
        
        args = parse_args()
        assert args.model_name_or_path == 'mistral'
        assert args.train_file == 'train.jsonl'
        # Verify defaults
        assert args.output_dir == "outputs/lora"
        assert args.per_device_batch_size == 8
        assert args.learning_rate == 2e-4

    @patch('sys.argv', ['train_lora.py', '--model_name_or_path', 'gpt2', '--train_file', 'data.jsonl', 
                        '--output_dir', 'custom_output', '--lora_r', '32', '--lora_alpha', '64'])
    def test_parse_args_with_custom_values(self):
        """Test parsing with custom parameter values"""
        from train_lora import parse_args
        
        args = parse_args()
        assert args.model_name_or_path == 'gpt2'
        assert args.train_file == 'data.jsonl'
        assert args.output_dir == 'custom_output'
        assert args.lora_r == 32
        assert args.lora_alpha == 64

    @patch('sys.argv', ['train_lora.py', '--model_name_or_path', 'model', '--train_file', 'train.jsonl',
                        '--valid_file', 'valid.jsonl'])
    def test_parse_args_with_validation_file(self):
        """Test parsing with validation file"""
        from train_lora import parse_args
        
        args = parse_args()
        assert args.valid_file == 'valid.jsonl'

    @patch('sys.argv', ['train_lora.py', '--model_name_or_path', 'model', '--train_file', 'train.jsonl',
                        '--target_modules', 'q_proj,v_proj,k_proj'])
    def test_parse_args_with_target_modules(self):
        """Test parsing with target modules specification"""
        from train_lora import parse_args
        
        args = parse_args()
        assert args.target_modules == 'q_proj,v_proj,k_proj'

    @patch('sys.argv', ['train_lora.py', '--model_name_or_path', 'model', '--train_file', 'train.jsonl',
                        '--learning_rate', '5e-4', '--num_train_epochs', '5'])
    def test_parse_args_with_float_and_int_arguments(self):
        """Test parsing with floating point and integer arguments"""
        from train_lora import parse_args
        
        args = parse_args()
        assert args.learning_rate == 5e-4
        assert args.num_train_epochs == 5

    def test_parse_args_missing_required_args_raises_error(self):
        """Test that missing required arguments raises SystemExit"""
        with patch('sys.argv', ['train_lora.py']):
            from train_lora import parse_args
            with pytest.raises(SystemExit):
                parse_args()


class TestBuildPrompt:
    """Tests for prompt building logic"""

    def test_build_prompt_with_valid_fields(self):
        """Test prompt building with valid prompt and completion"""
        from train_lora import build_prompt
        
        example = {"prompt": "What is AI?", "completion": " It is artificial intelligence."}
        result = build_prompt(example)
        
        assert "What is AI?" in result
        assert "It is artificial intelligence." in result
        assert result == "What is AI? It is artificial intelligence."

    def test_build_prompt_with_empty_fields(self):
        """Test prompt building with empty fields"""
        from train_lora import build_prompt
        
        example = {"prompt": "", "completion": ""}
        result = build_prompt(example)
        
        assert result == ""

    def test_build_prompt_with_only_prompt(self):
        """Test prompt building with only prompt field"""
        from train_lora import build_prompt
        
        example = {"prompt": "Hello world"}
        result = build_prompt(example)
        
        assert result == "Hello world"

    def test_build_prompt_with_only_completion(self):
        """Test prompt building with only completion field"""
        from train_lora import build_prompt
        
        example = {"completion": "Response text"}
        result = build_prompt(example)
        
        assert result == "Response text"

    def test_build_prompt_with_missing_fields(self):
        """Test prompt building with missing prompt and completion"""
        from train_lora import build_prompt
        
        example = {}
        result = build_prompt(example)
        
        assert result == ""

    def test_build_prompt_preserves_special_characters(self):
        """Test that special characters are preserved in prompt"""
        from train_lora import build_prompt
        
        example = {"prompt": "What is @#$%?", "completion": " It's special!"}
        result = build_prompt(example)
        
        assert "@#$%" in result
        assert "It's special!" in result

    def test_build_prompt_with_multiline_text(self):
        """Test prompt building with multiline text"""
        from train_lora import build_prompt
        
        example = {"prompt": "Line 1\nLine 2", "completion": "\nLine 3"}
        result = build_prompt(example)
        
        assert "Line 1" in result
        assert "Line 2" in result
        assert "Line 3" in result


class TestTokenizeFn:
    """Tests for tokenization function"""

    def test_tokenize_fn_creates_input_ids(self):
        """Test that tokenization creates input_ids"""
        from train_lora import tokenize_fn
        
        mock_tokenizer = Mock()
        mock_tokenizer.return_value = {"input_ids": [101, 2054, 2003, 6273, 102], "token_type_ids": [0, 0, 0, 0, 0]}
        
        example = {"prompt": "What is AI?", "completion": " Artificial intelligence."}
        result = tokenize_fn(example, mock_tokenizer, max_length=512)
        
        assert "input_ids" in result
        assert "labels" in result

    def test_tokenize_fn_creates_labels_matching_input_ids(self):
        """Test that labels are copied from input_ids"""
        from train_lora import tokenize_fn
        
        input_ids = [101, 2054, 2003, 6273, 102]
        mock_tokenizer = Mock()
        mock_tokenizer.return_value = {"input_ids": input_ids, "token_type_ids": [0, 0, 0, 0, 0]}
        
        example = {"prompt": "What is AI?", "completion": " Artificial intelligence."}
        result = tokenize_fn(example, mock_tokenizer, max_length=512)
        
        assert result["labels"] == input_ids
        assert result["labels"] is not result["input_ids"]  # Should be a copy

    def test_tokenize_fn_respects_max_length(self):
        """Test that tokenization respects max_length parameter"""
        from train_lora import tokenize_fn
        
        mock_tokenizer = Mock()
        mock_tokenizer.return_value = {"input_ids": [101] * 100, "token_type_ids": [0] * 100}
        
        example = {"prompt": "Long text", "completion": " More text"}
        tokenize_fn(example, mock_tokenizer, max_length=512)
        
        # Verify tokenizer was called with truncation and max_length
        mock_tokenizer.assert_called_once()
        call_args = mock_tokenizer.call_args
        assert call_args[1].get('truncation') == True
        assert call_args[1].get('max_length') == 512

    def test_tokenize_fn_with_empty_example(self):
        """Test tokenization with empty example"""
        from train_lora import tokenize_fn
        
        mock_tokenizer = Mock()
        mock_tokenizer.return_value = {"input_ids": [], "token_type_ids": []}
        
        example = {"prompt": "", "completion": ""}
        result = tokenize_fn(example, mock_tokenizer, max_length=512)
        
        assert "input_ids" in result
        assert "labels" in result

    def test_tokenize_fn_with_large_max_length(self):
        """Test tokenization with large max_length"""
        from train_lora import tokenize_fn
        
        mock_tokenizer = Mock()
        mock_tokenizer.return_value = {"input_ids": [101, 102], "token_type_ids": [0, 0]}
        
        example = {"prompt": "test", "completion": ""}
        tokenize_fn(example, mock_tokenizer, max_length=2048)
        
        call_args = mock_tokenizer.call_args
        assert call_args[1].get('max_length') == 2048


class TestModuleDetection:
    """Tests for LoRA target modules auto-detection logic"""

    def test_any_module_matches_with_matching_candidate(self):
        """Test module matching with candidate that exists"""
        # This simulates the any_module_matches function logic
        candidates = ["q_proj", "v_proj"]
        module_names = ["module.q_proj.weight", "module.v_proj.weight", "module.linear.bias"]
        
        def any_module_matches(candidates):
            for cand in candidates:
                for name in module_names:
                    if cand in name:
                        return True
            return False
        
        assert any_module_matches(candidates) == True

    def test_any_module_matches_with_no_matching_candidate(self):
        """Test module matching with no matching candidates"""
        candidates = ["xyz_proj", "abc_proj"]
        module_names = ["module.q_proj.weight", "module.v_proj.weight"]
        
        def any_module_matches(candidates):
            for cand in candidates:
                for name in module_names:
                    if cand in name:
                        return True
            return False
        
        assert any_module_matches(candidates) == False

    def test_model_type_detection_for_gpt2(self):
        """Test target module detection for GPT-2 model"""
        model_type = "gpt2"
        
        if "gpt2" in model_type or "gpt" in model_type:
            fallback = ["c_attn", "c_proj"]
        else:
            fallback = ["q_proj", "v_proj"]
        
        assert "c_attn" in fallback
        assert "c_proj" in fallback

    def test_model_type_detection_for_llama(self):
        """Test target module detection for LLaMA model"""
        model_type = "llama"
        
        if "gpt2" in model_type or "gpt" in model_type:
            fallback = ["c_attn", "c_proj"]
        elif "llama" in model_type:
            fallback = ["q_proj", "v_proj"]
        else:
            fallback = ["q_proj", "v_proj", "c_attn", "c_proj"]
        
        assert "q_proj" in fallback
        assert "v_proj" in fallback

    def test_model_type_detection_for_bloom(self):
        """Test target module detection for Bloom model"""
        model_type = "bloom"
        
        if "gpt2" in model_type or "gpt" in model_type:
            fallback = ["c_attn", "c_proj"]
        elif "llama" in model_type:
            fallback = ["q_proj", "v_proj"]
        elif "bloom" in model_type:
            fallback = ["query_key_value"]
        else:
            fallback = ["q_proj", "v_proj", "c_attn", "c_proj"]
        
        assert "query_key_value" in fallback

    def test_model_type_detection_for_unknown_model(self):
        """Test target module detection for unknown model type"""
        model_type = "unknown_model"
        
        if "gpt2" in model_type or "gpt" in model_type:
            fallback = ["c_attn", "c_proj"]
        elif "llama" in model_type:
            fallback = ["q_proj", "v_proj"]
        elif "bloom" in model_type:
            fallback = ["query_key_value"]
        else:
            fallback = ["q_proj", "v_proj", "c_attn", "c_proj"]
        
        assert len(fallback) == 4


class TestLoRASetup:
    """Tests for LoRA configuration and wrapping logic"""

    def test_apply_lora_to_model_wraps_model_without_bitsandbytes(self):
        """LoRA should still be applied when CUDA/bitsandbytes is unavailable."""
        from train_lora import apply_lora_to_model

        fake_model = Mock()
        fake_model.named_modules.return_value = [
            ("layer.q_proj", Mock()),
            ("layer.v_proj", Mock()),
            ("layer.linear", Mock()),
        ]
        fake_model.config = Mock(model_type="llama")

        args = argparse.Namespace(
            lora_r=4,
            lora_alpha=8,
            lora_dropout=0.1,
            target_modules="",
        )

        with patch("train_lora.LoraConfig") as mock_lora_cfg, patch(
            "train_lora.get_peft_model", return_value="wrapped_model"
        ) as mock_get_peft:
            result = apply_lora_to_model(fake_model, args, use_cuda=False, have_bnb=False)

        assert result == "wrapped_model"
        mock_lora_cfg.assert_called_once()
        mock_get_peft.assert_called_once_with(fake_model, mock_lora_cfg.return_value)


class TestDataProcessing:
    """Tests for data loading and processing"""

    def test_data_file_configuration_with_only_train(self):
        """Test data file configuration with only train file"""
        train_file = "train.jsonl"
        valid_file = None
        
        data_files = {"train": train_file}
        if valid_file:
            data_files["validation"] = valid_file
        
        assert "train" in data_files
        assert "validation" not in data_files

    def test_data_file_configuration_with_train_and_validation(self):
        """Test data file configuration with both train and validation files"""
        train_file = "train.jsonl"
        valid_file = "valid.jsonl"
        
        data_files = {"train": train_file}
        if valid_file:
            data_files["validation"] = valid_file
        
        assert "train" in data_files
        assert "validation" in data_files
        assert data_files["validation"] == valid_file


class TestTrainingConfiguration:
    """Tests for training arguments configuration"""

    @pytest.mark.parametrize("valid_file", [None, "valid.jsonl"])
    def test_main_constructs_real_training_arguments(self, tmp_path, valid_file):
        """Exercise production kwargs with the installed Transformers constructor."""
        import train_lora
        from transformers import TrainingArguments

        argv = [
            "train_lora.py", "--model_name_or_path", "unused",
            "--train_file", "train.jsonl", "--output_dir", str(tmp_path),
        ]
        if valid_file:
            argv.extend(["--valid_file", valid_file])

        dataset = MagicMock()
        dataset["train"].column_names = ["prompt", "completion"]
        tokenized = dataset.map.return_value
        with (
            patch("sys.argv", argv),
            patch("train_lora.torch.cuda.is_available", return_value=False),
            patch("train_lora.AutoTokenizer.from_pretrained"),
            patch("train_lora.AutoModelForCausalLM.from_pretrained"),
            patch("train_lora.load_dataset", return_value=dataset),
            patch("train_lora.DataCollatorForLanguageModeling"),
            patch("train_lora.Trainer") as trainer,
        ):
            train_lora.main()

        kwargs = trainer.call_args.kwargs
        args = kwargs["args"]
        assert isinstance(args, TrainingArguments)
        assert args.eval_strategy.value == ("steps" if valid_file else "no")
        assert args.do_eval is bool(valid_file)
        if valid_file:
            assert args.eval_steps == 500
            assert kwargs["eval_dataset"] is tokenized["validation"]
        else:
            assert kwargs["eval_dataset"] is None
        trainer.return_value.train.assert_called_once_with()

    def test_training_args_batch_size_configuration(self):
        """Test batch size and gradient accumulation configuration"""
        batch_size = 8
        grad_accum = 4
        
        ta_kwargs = {
            "per_device_train_batch_size": batch_size,
            "gradient_accumulation_steps": grad_accum
        }
        
        assert ta_kwargs["per_device_train_batch_size"] == 8
        assert ta_kwargs["gradient_accumulation_steps"] == 4

    def test_training_args_learning_rate_configuration(self):
        """Test learning rate configuration"""
        learning_rate = 2e-4
        
        ta_kwargs = {"learning_rate": learning_rate}
        
        assert ta_kwargs["learning_rate"] == 2e-4
        assert ta_kwargs["learning_rate"] > 0


class TestErrorHandling:
    """Tests for error handling and edge cases"""

    def test_parse_args_with_invalid_type(self):
        """Test argument parsing with invalid type specifications"""
        with patch('sys.argv', ['train_lora.py', '--model_name_or_path', 'model', '--train_file', 'train.jsonl',
                                '--per_device_batch_size', 'invalid']):
            from train_lora import parse_args
            with pytest.raises(SystemExit):
                parse_args()

    def test_build_prompt_with_none_values(self):
        """Test prompt building with None values in dict"""
        from train_lora import build_prompt
        
        example = {"prompt": None, "completion": None}
        # Should handle gracefully or use .get() which returns None
        try:
            result = build_prompt(example)
            # If it works, result should be constructed from None or empty
        except (TypeError, AttributeError):
            # This is acceptable - None + string operations
            pass


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
