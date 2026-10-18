# Test Coverage for Critical Code Paths

This document describes the comprehensive test suite created for critical business logic, authentication, data mutation, and configuration paths in the Ntech-ai project.

## Overview

The test suite covers three main components:
1. **C# Desktop Application** - Chat UI and dependency injection
2. **C# Core Library** - AI provider implementations
3. **Python Training Script** - Model fine-tuning logic

## C# Tests

### Project Structure

```
Ntech.AI.Core.Tests/
├── Providers/
│   └── OllamaProviderTests.cs

Ntech.AI.Desktop.Tests/
├── ViewModels/
│   └── MainWindowViewModelTests.cs
└── AppConfigurationTests.cs
```

### Running C# Tests

```bash
# Run all C# tests
dotnet test

# Run specific test project
dotnet test Ntech.AI.Core.Tests
dotnet test Ntech.AI.Desktop.Tests

# Run specific test class
dotnet test --filter "FullyQualifiedName~OllamaProviderTests"

# Run with verbose output
dotnet test -v detailed
```

## Test Suites

### 1. OllamaProviderTests (Critical Data Mutation Path)

**File:** `Ntech.AI.Core.Tests/Providers/OllamaProviderTests.cs`

**Purpose:** Tests the critical data mutation path for AI provider communication.

**Test Coverage:**

#### Constructor Tests
- `Constructor_WithDefaultParameters_InitializesCorrectly` - Verifies default initialization
- `Constructor_WithCustomParameters_StoresConfiguration` - Validates custom configuration storage

#### GetResponseAsync Tests (Critical - API Communication)
- `GetResponseAsync_WithValidMessages_ReturnsResponseContent` - Basic API functionality
- `GetResponseAsync_WithMultipleMessages_MaintainsConversationHistory` - Context preservation
- `GetResponseAsync_WithEmptyContent_HandlesMissingContent` - Edge case handling
- `GetResponseAsync_WithNullMessages_ThrowsOrHandlesGracefully` - Null safety
- `GetResponseAsync_WithSystemMessageRole_CorrectlyFormatsRole` - Message role handling
- `GetResponseAsync_ErrorHandling_CatchesExceptionAndReturnsErrorMessage` - Error resilience
- `GetResponseAsync_WithDifferentMessageRoles_MaintainsRoleTypes` - Role preservation
- `GetResponseAsync_WithLongContent_HandlesLargeTextProperly` - Large data handling
- `GetResponseAsync_WithSpecialCharacters_PreservesContent` - Content integrity
- `GetResponseAsync_WithMultipleConcurrentRequests_IsThreadSafe` - Thread safety

#### ValidateConnectionAsync Tests (Critical - Connectivity)
- `ValidateConnectionAsync_WithRunningService_ReturnsTrue` - Connection validation success
- `ValidateConnectionAsync_WithInvalidUrl_ReturnsFalse` - Invalid URL handling
- `ValidateConnectionAsync_WithUnreachableHost_ReturnsFalse` - Unreachable host handling

#### GetAvailableModelsAsync Tests (Data Mutation)
- `GetAvailableModelsAsync_ReturnsListOfModels` - Model list retrieval
- `GetAvailableModelsAsync_WithInvalidUrl_ReturnsEmptyList` - Graceful failure
- `GetAvailableModelsAsync_WithMultipleModels_ReturnsCombinedList` - Multiple model handling

**Key Assertions:**
- Response content is non-null and non-empty
- Error messages contain proper localization ("Hiba" for Hungarian errors)
- Connection validation returns boolean
- Model lists are properly constructed
- All exceptions are caught and handled gracefully

---

### 2. MainWindowViewModelTests (Critical Business Logic & Data Mutation)

**File:** `Ntech.AI.Desktop.Tests/ViewModels/MainWindowViewModelTests.cs`

**Purpose:** Tests critical chat message handling and UI state management.

**Test Coverage:**

#### Constructor Tests
- `Constructor_WithValidProvider_InitializesCorrectly` - Proper initialization
- `Constructor_WithNullProvider_ThrowsArgumentNullException` - Null safety

#### SendMessage Tests (Critical - User Interaction & Data Mutation)
- `SendMessage_WithValidMessage_AddsUserMessageToCollection` - Message collection mutation
- `SendMessage_WithValidMessage_ClearsInputMessageAfterSending` - Input clearing
- `SendMessage_WithValidMessage_RequestsResponseFromProvider` - Provider invocation
- `SendMessage_WithValidMessage_AddsAssistantResponseToCollection` - Response handling
- `SendMessage_WithEmptyMessage_DoesNotSendRequest` - Empty message validation
- `SendMessage_WithWhitespaceOnlyMessage_DoesNotSendRequest` - Whitespace validation
- `SendMessage_SetsLoadingStateDuringRequest` - Loading state management
- `SendMessage_WithProviderException_ClearsLoadingState` - Exception handling
- `SendMessage_WithProviderException_AddsErrorMessage` - Error messaging
- `SendMessage_MultipleSequentialMessages_MaintainsOrderAndHistory` - History preservation
- `SendMessage_IncludesAllPreviousMessagesInRequest` - Context preservation
- `SendMessage_WithProviderReturningNull_HandlesNullResponseGracefully` - Null handling
- `SendMessage_WithProviderReturningEmptyString_HandlesEmptyResponseGracefully` - Empty response
- `SendMessage_WithLongConversation_HandlesLargeHistoryCorrectly` - Performance with 50+ messages
- `SendMessage_WithSpecialCharactersInMessage_PreservesContent` - Content preservation
- `SendMessage_StateManagement_IsLoadingFalseAfterCompletion` - State cleanup

#### NewChat Tests (Data Mutation)
- `NewChat_ClearsAllMessages` - Message collection clearing
- `NewChat_ClearsInputMessage` - Input field clearing
- `NewChat_MultipleTimesInSequence_WorksCorrectly` - Repeated reset functionality

#### Observable Collection Tests
- `Messages_CollectionType_IsObservableCollection` - UI binding compliance

**Key Assertions:**
- Messages are added to collection in correct order
- Input message is cleared after sending
- Provider is called with correct parameters
- Loading state is properly managed (true during request, false after)
- Error messages include exception details
- Special characters and long content are preserved
- Multiple rapid calls don't corrupt state

---

### 3. AppConfigurationTests (Critical Authentication & Configuration)

**File:** `Ntech.AI.Desktop.Tests/AppConfigurationTests.cs`

**Purpose:** Tests dependency injection configuration and service setup.

**Test Coverage:**

#### Service Registration Tests
- `ConfigureServices_RegistersOllamaProviderAsSingleton` - Provider registration
- `ConfigureServices_RegistersMainWindowViewModelAsSingleton` - ViewModel registration
- `ConfigureServices_RegistersConfiguration` - Configuration registration

#### Configuration Tests
- `ConfigureServices_WithDefaultOllamaConfig_UsesDefaults` - Default value handling
- `ConfigureServices_WithCustomBaseUrl_UsesProvidedValue` - Custom URL configuration
- `ConfigureServices_WithCustomModel_UsesProvidedValue` - Custom model configuration

#### Singleton Tests (Critical for Application Stability)
- `ConfigureServices_ProviderInstance_IsSameSingletonInstance` - Provider singleton verification
- `ConfigureServices_ViewModelInstance_IsSameSingletonInstance` - ViewModel singleton verification

#### Integration Tests
- `ConfigureServices_AllServicesRegistered_CanResolveViewModel` - End-to-end resolution
- `DependencyInjection_ViewModelWithProvider_ProviderIsNotNull` - DI correctness

**Key Assertions:**
- All services are registered as singletons
- Configuration is properly read from appsettings.json
- Default values are used when configuration is missing
- Each service resolution returns the same instance
- No null references in the dependency graph

---

## Python Tests

### Project Structure

```
tests/
├── test_train_lora.py
├── requirements.txt
└── __init__.py
```

### Running Python Tests

```bash
# Install test dependencies
pip install -r tests/requirements.txt

# Run all tests
pytest tests/test_train_lora.py -v

# Run specific test class
pytest tests/test_train_lora.py::TestParseArgs -v

# Run with coverage
pytest tests/test_train_lora.py --cov=train_lora --cov-report=html

# Run specific test
pytest tests/test_train_lora.py::TestParseArgs::test_parse_args_with_required_args_only -v
```

### Test Suites

#### 1. TestParseArgs (Business Logic - Argument Validation)

**Purpose:** Tests command-line argument parsing and validation.

**Tests:**
- `test_parse_args_with_required_args_only` - Minimal valid arguments
- `test_parse_args_with_custom_values` - Custom parameter values
- `test_parse_args_with_validation_file` - Optional validation file
- `test_parse_args_with_target_modules` - LoRA target modules specification
- `test_parse_args_with_float_and_int_arguments` - Numeric type parsing
- `test_parse_args_missing_required_args_raises_error` - Validation error

**Key Assertions:**
- Required arguments are parsed correctly
- Default values are applied
- Type conversion works for float and int values
- Missing required arguments raise SystemExit

---

#### 2. TestBuildPrompt (Business Logic - Data Preparation)

**Purpose:** Tests prompt construction from prompt and completion fields.

**Tests:**
- `test_build_prompt_with_valid_fields` - Normal concatenation
- `test_build_prompt_with_empty_fields` - Edge case: empty inputs
- `test_build_prompt_with_only_prompt` - Missing completion
- `test_build_prompt_with_only_completion` - Missing prompt
- `test_build_prompt_with_missing_fields` - Missing both fields
- `test_build_prompt_preserves_special_characters` - Content integrity
- `test_build_prompt_with_multiline_text` - Multiline handling

**Key Assertions:**
- Prompt and completion are concatenated correctly
- Special characters are preserved
- Missing fields are handled gracefully
- Output format is correct for tokenization

---

#### 3. TestTokenizeFn (Critical Data Mutation - Tokenization)

**Purpose:** Tests conversion of text to tokenized tensors.

**Tests:**
- `test_tokenize_fn_creates_input_ids` - Token generation
- `test_tokenize_fn_creates_labels_matching_input_ids` - Label creation
- `test_tokenize_fn_respects_max_length` - Sequence length limits
- `test_tokenize_fn_with_empty_example` - Empty input handling
- `test_tokenize_fn_with_large_max_length` - Large sequences

**Key Assertions:**
- input_ids are created from text
- labels are identical copies of input_ids
- max_length parameter is respected
- Truncation is enabled for safety

---

#### 4. TestModuleDetection (Business Logic - Model Adaptation)

**Purpose:** Tests auto-detection of LoRA target modules for different model architectures.

**Tests:**
- `test_any_module_matches_with_matching_candidate` - Module name matching
- `test_any_module_matches_with_no_matching_candidate` - No matches
- `test_model_type_detection_for_gpt2` - GPT-2 targets
- `test_model_type_detection_for_llama` - LLaMA targets
- `test_model_type_detection_for_bloom` - Bloom targets
- `test_model_type_detection_for_unknown_model` - Fallback for unknown models

**Key Assertions:**
- Correct target modules for each model type
- Fallback mechanism for unknown models
- Module name matching logic is correct

---

#### 5. TestDataProcessing (Data Mutation - Dataset Configuration)

**Purpose:** Tests dataset file configuration.

**Tests:**
- `test_data_file_configuration_with_only_train` - Train-only setup
- `test_data_file_configuration_with_train_and_validation` - With validation set

**Key Assertions:**
- Data files dict is properly constructed
- Validation file is conditionally added

---

#### 6. TestTrainingConfiguration (Configuration - Training Parameters)

**Purpose:** Tests training arguments setup.

**Tests:**
- `test_training_args_with_validation_file` - Eval parameters when validation exists
- `test_training_args_without_validation_file` - No eval when no validation
- `test_training_args_batch_size_configuration` - Batch settings
- `test_training_args_learning_rate_configuration` - Learning rate setup

**Key Assertions:**
- Evaluation strategy only added when validation file exists
- Batch size and learning rate are properly set
- All training parameters are positive values

---

#### 7. TestErrorHandling (Error Resilience)

**Purpose:** Tests error handling and edge cases.

**Tests:**
- `test_parse_args_with_invalid_type` - Type validation
- `test_build_prompt_with_none_values` - Null handling

**Key Assertions:**
- Invalid argument types raise SystemExit
- None values are handled gracefully or raise TypeError

---

## Critical Paths Summary

| Path | Category | Priority | Test File | Tests | Status |
|------|----------|----------|-----------|-------|--------|
| OllamaProvider.GetResponseAsync | Data Mutation | Critical | OllamaProviderTests.cs | 10 | ✓ Done |
| OllamaProvider.ValidateConnectionAsync | Business Logic | High | OllamaProviderTests.cs | 3 | ✓ Done |
| OllamaProvider.GetAvailableModelsAsync | Data Mutation | High | OllamaProviderTests.cs | 3 | ✓ Done |
| MainWindowViewModel.SendMessage | Data Mutation | Critical | MainWindowViewModelTests.cs | 16 | ✓ Done |
| MainWindowViewModel.NewChat | Data Mutation | High | MainWindowViewModelTests.cs | 3 | ✓ Done |
| App.ConfigureServices | Auth/Config | High | AppConfigurationTests.cs | 10 | ✓ Done |
| train_lora.parse_args | Business Logic | High | test_train_lora.py | 6 | ✓ Done |
| train_lora.build_prompt | Business Logic | High | test_train_lora.py | 7 | ✓ Done |
| train_lora.tokenize_fn | Data Mutation | Critical | test_train_lora.py | 5 | ✓ Done |
| train_lora.main (module detection) | Business Logic | High | test_train_lora.py | 6 | ✓ Done |
| train_lora.main (training config) | Configuration | High | test_train_lora.py | 4 | ✓ Done |

**Total Test Cases:** 73

---

## Test Execution Checklist

- [ ] Install C# dependencies: `dotnet restore`
- [ ] Install Python dependencies: `pip install -r tests/requirements.txt`
- [ ] Run C# tests: `dotnet test`
- [ ] Run Python tests: `pytest tests/`
- [ ] Review coverage reports
- [ ] Fix any failing tests
- [ ] Commit test files

## Coverage Goals

- **OllamaProvider**: 90%+ coverage of public methods
- **MainWindowViewModel**: 95%+ coverage of command execution paths
- **App Configuration**: 100% coverage of service registration
- **train_lora.py**: 85%+ coverage of critical functions

## Best Practices

1. **Run tests locally** before committing
2. **Use mocks** for external dependencies (HTTP, file system)
3. **Test edge cases** (null, empty, special characters)
4. **Verify error handling** with try-catch scenarios
5. **Test state management** with sequential operations
6. **Include performance tests** for long conversations/datasets

## CI/CD Integration

Add to your CI/CD pipeline:

```yaml
# Example GitHub Actions
- name: Run C# Tests
  run: dotnet test --verbosity normal

- name: Run Python Tests
  run: pytest tests/ -v --cov
```

## Next Steps

1. **Generate coverage reports** using `dotnet test /p:CollectCoverage=true`
2. **Fix any failing tests** and iterate
3. **Add integration tests** for end-to-end scenarios
4. **Document test patterns** for future development
5. **Set up CI/CD** to run tests on every commit
