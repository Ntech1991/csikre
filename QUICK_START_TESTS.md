# Quick Start Guide - Test Execution

## TL;DR - Run Tests Now

### C# Tests (58 tests)
```bash
cd path/to/repo
dotnet test
```

### Python Tests (35+ tests)
```bash
pip install -r tests/requirements.txt
pytest tests/test_train_lora.py -v
```

---

## What Tests Cover

### Critical Paths (Tested)
✅ **Data Mutation** - Chat messages, tokenization, API responses
✅ **Business Logic** - Argument parsing, prompt building, module detection
✅ **Authentication** - Dependency injection, service configuration
✅ **Error Handling** - Null checks, exceptions, edge cases
✅ **State Management** - Loading flags, collection changes
✅ **Concurrency** - Thread safety for concurrent requests

### Test Breakdown
- **OllamaProvider** (HTTP communication): 16 tests
- **MainWindowViewModel** (Chat UI): 32 tests  
- **App Configuration** (Dependency Injection): 10 tests
- **train_lora.py** (Training script): 35+ tests

---

## File Locations

| Component | File Path | Tests |
|-----------|-----------|-------|
| AI Provider | `Ntech.AI.Core/Providers/OllamaProvider.cs` | `OllamaProviderTests.cs` |
| Chat ViewModel | `Ntech.AI.Desktop/MainWindowViewModel.cs` | `MainWindowViewModelTests.cs` |
| App Setup | `Ntech.AI.Desktop/App.xaml.cs` | `AppConfigurationTests.cs` |
| Training Script | `train_lora.py` | `tests/test_train_lora.py` |

---

## Test Results Expected

When all tests pass:
```
C# Tests:
  OllamaProviderTests ........... 16 passed
  MainWindowViewModelTests ...... 32 passed
  AppConfigurationTests ......... 10 passed
  Total: 58 passed

Python Tests:
  TestParseArgs ................ 6 passed
  TestBuildPrompt .............. 7 passed
  TestTokenizeFn ............... 5 passed
  TestModuleDetection .......... 6 passed
  TestDataProcessing ........... 2 passed
  TestTrainingConfiguration .... 4 passed
  TestErrorHandling ............ 2 passed
  Total: 35+ passed

All tests passing ✓
```

---

## Key Test Scenarios

### OllamaProvider Tests
- ✓ Valid message handling
- ✓ Connection validation
- ✓ Model availability
- ✓ Error resilience
- ✓ Thread safety

### MainWindowViewModel Tests
- ✓ Message collection
- ✓ Loading state management
- ✓ Provider communication
- ✓ Error handling
- ✓ Chat reset functionality

### Dependency Injection Tests
- ✓ Service registration
- ✓ Singleton verification
- ✓ Configuration loading
- ✓ Custom value handling

### Training Script Tests
- ✓ Argument parsing
- ✓ Prompt building
- ✓ Tokenization
- ✓ Module detection
- ✓ Training configuration

---

## Documentation

- **TESTING.md** - Comprehensive test documentation with all test descriptions
- **TEST_COVERAGE_SUMMARY.md** - Executive summary and statistics
- **This file** - Quick reference guide

---

## Next Steps

1. Run the full test suite: `dotnet test && pytest tests/`
2. Check coverage reports: `dotnet test /p:CollectCoverage=true`
3. Integrate with CI/CD (GitHub Actions example in TEST_COVERAGE_SUMMARY.md)
4. Monitor test results on every commit

---

## Support

For detailed information about specific tests, see **TESTING.md**
For architecture and coverage statistics, see **TEST_COVERAGE_SUMMARY.md**
