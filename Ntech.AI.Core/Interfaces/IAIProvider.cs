using Ntech.AI.Common.Models;

namespace Ntech.AI.Core.Interfaces
{
    /// <summary>
    /// Interface for AI providers
    /// </summary>
    public interface IAIProvider
    {
        /// <summary>
        /// Get response from AI provider
        /// </summary>
        Task<string> GetResponseAsync(List<ChatMessage> messages);

        /// <summary>
        /// Validate provider connection
        /// </summary>
        Task<bool> ValidateConnectionAsync();

        /// <summary>
        /// Get available models from provider
        /// </summary>
        Task<List<string>> GetAvailableModelsAsync();
    }
}
