using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ntech.AI.Common.Models;
using Ntech.AI.Core.Interfaces;

namespace Ntech.AI.Desktop
{
    /// <summary>
    /// Main window ViewModel
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IAIProvider _aiProvider;

        [ObservableProperty]
        private ObservableCollection<ChatMessage> messages = new();

        [ObservableProperty]
        private string? inputMessage;

        [ObservableProperty]
        private bool isLoading;

        public MainWindowViewModel(IAIProvider aiProvider)
        {
            _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
        }

        [RelayCommand]
        public async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(InputMessage))
                return;

            var userMessage = new ChatMessage
            {
                Content = InputMessage,
                Role = MessageRole.User
            };

            Messages.Add(userMessage);
            InputMessage = string.Empty;

            IsLoading = true;

            try
            {
                var response = await _aiProvider.GetResponseAsync(Messages.ToList());
                var aiMessage = new ChatMessage
                {
                    Content = response,
                    Role = MessageRole.Assistant
                };

                Messages.Add(aiMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = new ChatMessage
                {
                    Content = $"Hiba: {ex.Message}",
                    Role = MessageRole.System
                };
                Messages.Add(errorMessage);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void NewChat()
        {
            Messages.Clear();
            InputMessage = string.Empty;
        }
    }
}