using Xunit;
using Moq;
using Ntech.AI.Desktop;
using Ntech.AI.Core.Interfaces;
using Ntech.AI.Common.Models;

namespace Ntech.AI.Desktop.Tests.ViewModels
{
    /// <summary>
    /// Comprehensive tests for MainWindowViewModel critical data mutation and business logic
    /// </summary>
    public class MainWindowViewModelTests
    {
        private readonly Mock<IAIProvider> _mockAiProvider;
        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModelTests()
        {
            _mockAiProvider = new Mock<IAIProvider>();
            _viewModel = new MainWindowViewModel(_mockAiProvider.Object);
        }

        [Fact]
        public void Constructor_WithValidProvider_InitializesCorrectly()
        {
            // Arrange
            var mockProvider = new Mock<IAIProvider>();

            // Act
            var viewModel = new MainWindowViewModel(mockProvider.Object);

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Messages);
            Assert.Empty(viewModel.Messages);
            Assert.Null(viewModel.InputMessage);
            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public void Constructor_WithNullProvider_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(null!));
        }

        [Fact]
        public async Task SendMessage_WithValidMessage_AddsUserMessageToCollection()
        {
            // Arrange
            var testMessage = "Hello, AI!";
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync("Hello, User!");
            _viewModel.InputMessage = testMessage;

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            Assert.NotEmpty(_viewModel.Messages);
            var userMessage = _viewModel.Messages.First();
            Assert.Equal(testMessage, userMessage.Content);
            Assert.Equal(MessageRole.User, userMessage.Role);
        }

        [Fact]
        public async Task SendMessage_WithValidMessage_ClearsInputMessageAfterSending()
        {
            // Arrange
            _viewModel.InputMessage = "Test message";
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync("Response");

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            Assert.Empty(_viewModel.InputMessage ?? "");
        }

        [Fact]
        public async Task SendMessage_WithValidMessage_RequestsResponseFromProvider()
        {
            // Arrange
            var testMessage = "What is AI?";
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync("AI is artificial intelligence");
            _viewModel.InputMessage = testMessage;

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            _mockAiProvider.Verify(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()), Times.Once);
        }

        [Fact]
        public async Task SendMessage_WithValidMessage_AddsAssistantResponseToCollection()
        {
            // Arrange
            var expectedResponse = "This is the AI response";
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync(expectedResponse);
            _viewModel.InputMessage = "Test";

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            var assistantMessage = _viewModel.Messages.LastOrDefault();
            Assert.NotNull(assistantMessage);
            Assert.Equal(expectedResponse, assistantMessage.Content);
            Assert.Equal(MessageRole.Assistant, assistantMessage.Role);
        }

        [Fact]
        public async Task SendMessage_WithEmptyMessage_DoesNotSendRequest()
        {
            // Arrange
            _viewModel.InputMessage = string.Empty;

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            _mockAiProvider.Verify(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()), Times.Never);
            Assert.Empty(_viewModel.Messages);
        }

        [Fact]
        public async Task SendMessage_WithWhitespaceOnlyMessage_DoesNotSendRequest()
        {
            // Arrange
            _viewModel.InputMessage = "   \t\n   ";

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            _mockAiProvider.Verify(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()), Times.Never);
            Assert.Empty(_viewModel.Messages);
        }

        [Fact]
        public async Task SendMessage_SetsLoadingStateDuringRequest()
        {
            // Arrange
            var loadingStatesDuringRequest = new List<bool>();
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .Callback(() => loadingStatesDuringRequest.Add(_viewModel.IsLoading))
                .ReturnsAsync("Response");
            _viewModel.InputMessage = "Test";

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            Assert.True(loadingStatesDuringRequest.FirstOrDefault()); // Should be true during request
            Assert.False(_viewModel.IsLoading); // Should be false after request
        }

        [Fact]
        public async Task SendMessage_WithProviderException_ClearsLoadingState()
        {
            // Arrange
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ThrowsAsync(new Exception("Provider error"));
            _viewModel.InputMessage = "Test";

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            Assert.False(_viewModel.IsLoading);
        }

        [Fact]
        public async Task SendMessage_WithProviderException_AddsErrorMessage()
        {
            // Arrange
            var errorMessage = "Test provider error";
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ThrowsAsync(new Exception(errorMessage));
            _viewModel.InputMessage = "Test";

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            var errorMsg = _viewModel.Messages.LastOrDefault();
            Assert.NotNull(errorMsg);
            Assert.Equal(MessageRole.System, errorMsg.Role);
            Assert.Contains("Hiba", errorMsg.Content); // Error in Hungarian
            Assert.Contains(errorMessage, errorMsg.Content);
        }

        [Fact]
        public async Task SendMessage_MultipleSequentialMessages_MaintainsOrderAndHistory()
        {
            // Arrange
            var responses = new Queue<string>(new[] { "Response 1", "Response 2", "Response 3" });
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync(() => responses.Dequeue());

            // Act
            for (int i = 1; i <= 3; i++)
            {
                _viewModel.InputMessage = $"Message {i}";
                await _viewModel.SendMessageCommand.ExecuteAsync(null);
            }

            // Assert
            Assert.Equal(6, _viewModel.Messages.Count); // 3 user + 3 assistant
            Assert.Equal(MessageRole.User, _viewModel.Messages[0].Role);
            Assert.Equal(MessageRole.Assistant, _viewModel.Messages[1].Role);
            Assert.Equal("Message 1", _viewModel.Messages[0].Content);
        }

        [Fact]
        public async Task SendMessage_IncludesAllPreviousMessagesInRequest()
        {
            // Arrange
            List<ChatMessage> capturedMessages = null!;
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .Callback<List<ChatMessage>>(msgs => capturedMessages = msgs)
                .ReturnsAsync("Response");
            _viewModel.InputMessage = "First";
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            _viewModel.InputMessage = "Second";

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            Assert.NotNull(capturedMessages);
            Assert.True(capturedMessages.Count >= 2); // Should include previous messages
        }

        [Fact]
        public void NewChat_ClearsAllMessages()
        {
            // Arrange
            _viewModel.Messages.Add(new ChatMessage { Role = MessageRole.User, Content = "Message 1" });
            _viewModel.Messages.Add(new ChatMessage { Role = MessageRole.Assistant, Content = "Response 1" });

            // Act
            _viewModel.NewChatCommand.Execute(null);

            // Assert
            Assert.Empty(_viewModel.Messages);
        }

        [Fact]
        public void NewChat_ClearsInputMessage()
        {
            // Arrange
            _viewModel.InputMessage = "Some text";

            // Act
            _viewModel.NewChatCommand.Execute(null);

            // Assert
            Assert.Empty(_viewModel.InputMessage ?? "");
        }

        [Fact]
        public void NewChat_MultipleTimesInSequence_WorksCorrectly()
        {
            // Arrange
            _viewModel.Messages.Add(new ChatMessage { Role = MessageRole.User, Content = "Message" });
            _viewModel.InputMessage = "Input";

            // Act
            _viewModel.NewChatCommand.Execute(null);
            Assert.Empty(_viewModel.Messages);

            _viewModel.Messages.Add(new ChatMessage { Role = MessageRole.User, Content = "Message 2" });
            _viewModel.InputMessage = "Input 2";
            _viewModel.NewChatCommand.Execute(null);

            // Assert
            Assert.Empty(_viewModel.Messages);
            Assert.Empty(_viewModel.InputMessage ?? "");
        }

        [Fact]
        public async Task SendMessage_WithProviderReturningNull_HandlesNullResponseGracefully()
        {
            // Arrange
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync((string)null!);
            _viewModel.InputMessage = "Test";

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.Messages.Count >= 2); // User + response (even if null)
        }

        [Fact]
        public async Task SendMessage_WithProviderReturningEmptyString_HandlesEmptyResponseGracefully()
        {
            // Arrange
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync(string.Empty);
            _viewModel.InputMessage = "Test";

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            Assert.NotEmpty(_viewModel.Messages);
        }

        [Fact]
        public async Task SendMessage_WithLongConversation_HandlesLargeHistoryCorrectly()
        {
            // Arrange
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync("Response");

            // Act
            for (int i = 0; i < 50; i++)
            {
                _viewModel.InputMessage = $"Message {i}";
                await _viewModel.SendMessageCommand.ExecuteAsync(null);
            }

            // Assert
            Assert.Equal(100, _viewModel.Messages.Count); // 50 user + 50 assistant
        }

        [Fact]
        public async Task SendMessage_WithSpecialCharactersInMessage_PreservesContent()
        {
            // Arrange
            var specialMessage = "Test with special chars: @#$%^&*()_+-=[]{}|;:',.<>?/~`";
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync("Response");
            _viewModel.InputMessage = specialMessage;

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            var userMessage = _viewModel.Messages.First(m => m.Role == MessageRole.User);
            Assert.Equal(specialMessage, userMessage.Content);
        }

        [Fact]
        public async Task SendMessage_StateManagement_IsLoadingFalseAfterCompletion()
        {
            // Arrange
            _mockAiProvider.Setup(p => p.GetResponseAsync(It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync("Response");
            _viewModel.InputMessage = "Test";

            // Act
            await _viewModel.SendMessageCommand.ExecuteAsync(null);

            // Assert
            Assert.False(_viewModel.IsLoading);
        }

        [Fact]
        public void Messages_CollectionType_IsObservableCollection()
        {
            // Assert
            Assert.NotNull(_viewModel.Messages);
            // ObservableCollection implements INotifyCollectionChanged
            var notifyCollection = _viewModel.Messages as System.Collections.Specialized.INotifyCollectionChanged;
            Assert.NotNull(notifyCollection);
        }
    }
}
