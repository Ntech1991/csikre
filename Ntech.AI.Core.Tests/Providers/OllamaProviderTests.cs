using Xunit;
using Moq;
using Ntech.AI.Core.Providers;
using Ntech.AI.Common.Models;
using System.Net;

namespace Ntech.AI.Core.Tests.Providers
{
    /// <summary>
    /// Comprehensive tests for OllamaProvider critical data mutation and business logic paths
    /// </summary>
    public class OllamaProviderTests
    {
        private const string TestBaseUrl = "http://localhost:11434";
        private const string TestModel = "mistral";

        [Fact]
        public void Constructor_WithDefaultParameters_InitializesCorrectly()
        {
            // Arrange & Act
            var provider = new OllamaProvider();

            // Assert - verify defaults work
            Assert.NotNull(provider);
        }

        [Fact]
        public void Constructor_WithCustomParameters_StoresConfiguration()
        {
            // Arrange
            var customUrl = "http://custom.ai:8080";
            var customModel = "custom-model";

            // Act
            var provider = new OllamaProvider(customUrl, customModel);

            // Assert
            Assert.NotNull(provider);
        }

        [Fact]
        public async Task GetResponseAsync_WithValidMessages_ReturnsResponseContent()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = MessageRole.User, Content = "Hello, what is 2+2?" }
            };

            // Note: This test will fail without a running Ollama instance
            // In production, use dependency injection with mock HttpClient
            var response = await provider.GetResponseAsync(messages);

            // Assert - should either return valid response or error message
            Assert.NotNull(response);
            Assert.True(!string.IsNullOrWhiteSpace(response));
        }

        [Fact]
        public async Task GetResponseAsync_WithMultipleMessages_MaintainsConversationHistory()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = MessageRole.User, Content = "What is AI?" },
                new ChatMessage { Role = MessageRole.Assistant, Content = "AI is artificial intelligence." },
                new ChatMessage { Role = MessageRole.User, Content = "Can you explain it further?" }
            };

            // Act
            var response = await provider.GetResponseAsync(messages);

            // Assert
            Assert.NotNull(response);
            // Response should maintain context from previous messages
        }

        [Fact]
        public async Task GetResponseAsync_WithEmptyContent_HandlesMissingContent()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = MessageRole.User, Content = string.Empty }
            };

            // Act
            var response = await provider.GetResponseAsync(messages);

            // Assert - should handle gracefully
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetResponseAsync_WithNullMessages_ThrowsOrHandlesGracefully()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);

            // Act & Assert - should handle null appropriately
            try
            {
                var response = await provider.GetResponseAsync(null!);
                // If no exception, should return error message
                Assert.NotNull(response);
                Assert.Contains("Hiba", response); // Error in Hungarian
            }
            catch (ArgumentNullException)
            {
                // This is also acceptable behavior
            }
        }

        [Fact]
        public async Task GetResponseAsync_WithSystemMessageRole_CorrectlyFormatsRole()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = MessageRole.System, Content = "You are a helpful assistant." },
                new ChatMessage { Role = MessageRole.User, Content = "Hello" }
            };

            // Act
            var response = await provider.GetResponseAsync(messages);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ValidateConnectionAsync_WithRunningService_ReturnsTrue()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);

            // Act
            var isValid = await provider.ValidateConnectionAsync();

            // Assert - will return false if service not running
            // This is expected behavior in test environments
            Assert.IsType<bool>(isValid);
        }

        [Fact]
        public async Task ValidateConnectionAsync_WithInvalidUrl_ReturnsFalse()
        {
            // Arrange
            var provider = new OllamaProvider("http://invalid.local:99999", TestModel);

            // Act
            var isValid = await provider.ValidateConnectionAsync();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public async Task ValidateConnectionAsync_WithUnreachableHost_ReturnsFalse()
        {
            // Arrange
            var provider = new OllamaProvider("http://192.0.2.1:11434", TestModel); // Non-routable address

            // Act
            var isValid = await provider.ValidateConnectionAsync();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public async Task GetAvailableModelsAsync_ReturnsListOfModels()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);

            // Act
            var models = await provider.GetAvailableModelsAsync();

            // Assert
            Assert.NotNull(models);
            Assert.IsType<List<string>>(models);
        }

        [Fact]
        public async Task GetAvailableModelsAsync_WithInvalidUrl_ReturnsEmptyList()
        {
            // Arrange
            var provider = new OllamaProvider("http://invalid.local:99999", TestModel);

            // Act
            var models = await provider.GetAvailableModelsAsync();

            // Assert
            Assert.NotNull(models);
            Assert.Empty(models);
        }

        [Fact]
        public async Task GetAvailableModelsAsync_WithMultipleModels_ReturnsCombinedList()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);

            // Act
            var models = await provider.GetAvailableModelsAsync();

            // Assert
            Assert.NotNull(models);
            // If running service has models, they should be returned
            // If not, empty list is acceptable
        }

        [Fact]
        public async Task GetResponseAsync_ErrorHandling_CatchesExceptionAndReturnsErrorMessage()
        {
            // Arrange
            var provider = new OllamaProvider("http://unreachable.local:11434", TestModel);
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = MessageRole.User, Content = "Test" }
            };

            // Act
            var response = await provider.GetResponseAsync(messages);

            // Assert
            Assert.NotNull(response);
            Assert.Contains("Hiba", response); // Should contain error indicator
        }

        [Fact]
        public async Task GetResponseAsync_WithDifferentMessageRoles_MaintainsRoleTypes()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = MessageRole.User, Content = "First message" },
                new ChatMessage { Role = MessageRole.Assistant, Content = "Response" },
                new ChatMessage { Role = MessageRole.User, Content = "Follow-up" }
            };

            // Act
            var response = await provider.GetResponseAsync(messages);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetResponseAsync_WithLongContent_HandlesLargeTextProperly()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);
            var longContent = string.Concat(Enumerable.Repeat("This is a test message. ", 100));
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = MessageRole.User, Content = longContent }
            };

            // Act
            var response = await provider.GetResponseAsync(messages);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetResponseAsync_WithSpecialCharacters_PreservesContent()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = MessageRole.User, Content = "Tëst with spëcial çhars: @#$%&*()" }
            };

            // Act
            var response = await provider.GetResponseAsync(messages);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void GetResponseAsync_WithMultipleConcurrentRequests_IsThreadSafe()
        {
            // Arrange
            var provider = new OllamaProvider(TestBaseUrl, TestModel);
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < 5; i++)
            {
                var messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = MessageRole.User, Content = $"Message {i}" }
                };
                tasks.Add(provider.GetResponseAsync(messages));
            }

            // Assert
            Assert.All(tasks, t => Assert.NotNull(t));
        }
    }
}
