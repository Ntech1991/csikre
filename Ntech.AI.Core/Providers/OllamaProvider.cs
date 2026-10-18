using Ntech.AI.Common.Models;
using Ntech.AI.Core.Interfaces;
using System.Net.Http.Json;

namespace Ntech.AI.Core.Providers
{
    /// <summary>
    /// Ollama AI Provider - Local AI models
    /// </summary>
    public class OllamaProvider : IAIProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _model;

        public OllamaProvider(string baseUrl = "http://localhost:11434", string model = "mistral")
        {
            _baseUrl = baseUrl;
            _model = model;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Get response from Ollama API
        /// </summary>
        public async Task<string> GetResponseAsync(List<ChatMessage> messages)
        {
            try
            {
                var request = new
                {
                    model = _model,
                    messages = messages.Select(m => new
                    {
                        role = m.Role.ToString().ToLower(),
                        content = m.Content
                    }).ToList(),
                    stream = false
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/api/chat",
                    request
                );

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
                return result?.Message?.Content ?? "Hiba: Üres válasz";
            }
            catch (Exception ex)
            {
                return $"Hiba: {ex.Message}";
            }
        }

        /// <summary>
        /// Validate Ollama connection
        /// </summary>
        public async Task<bool> ValidateConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get available models from Ollama
        /// </summary>
        public async Task<List<string>> GetAvailableModelsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaTagsResponse>();
                return result?.Models?.Select(m => m.Name).ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private class OllamaResponse
        {
            public OllamaMessage? Message { get; set; }
        }

        private class OllamaMessage
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        private class OllamaTagsResponse
        {
            public List<OllamaModel>? Models { get; set; }
        }

        private class OllamaModel
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}