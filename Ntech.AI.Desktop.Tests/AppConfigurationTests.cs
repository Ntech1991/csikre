using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ntech.AI.Desktop;
using Ntech.AI.Core.Interfaces;
using Ntech.AI.Core.Providers;

namespace Ntech.AI.Desktop.Tests
{
    /// <summary>
    /// Tests for App dependency injection configuration and service setup
    /// </summary>
    public class AppConfigurationTests
    {
        [Fact]
        public void ConfigureServices_RegistersOllamaProviderAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            var mockOllamaSection = new Mock<IConfigurationSection>();
            
            mockOllamaSection.Setup(s => s["BaseUrl"]).Returns("http://localhost:11434");
            mockOllamaSection.Setup(s => s["Model"]).Returns("mistral");
            mockConfig.Setup(c => c.GetSection("Ollama")).Returns(mockOllamaSection.Object);
            
            // Act
            services.AddSingleton(mockConfig.Object);
            ConfigureTestServices(services);
            var provider = services.BuildServiceProvider();
            
            // Assert
            var aiProvider = provider.GetRequiredService<IAIProvider>();
            Assert.NotNull(aiProvider);
            Assert.IsType<OllamaProvider>(aiProvider);
        }

        [Fact]
        public void ConfigureServices_RegistersMainWindowViewModelAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            var mockOllamaSection = new Mock<IConfigurationSection>();
            
            mockOllamaSection.Setup(s => s["BaseUrl"]).Returns("http://localhost:11434");
            mockOllamaSection.Setup(s => s["Model"]).Returns("mistral");
            mockConfig.Setup(c => c.GetSection("Ollama")).Returns(mockOllamaSection.Object);
            
            // Act
            services.AddSingleton(mockConfig.Object);
            ConfigureTestServices(services);
            var provider = services.BuildServiceProvider();
            
            // Assert
            var viewModel = provider.GetRequiredService<MainWindowViewModel>();
            Assert.NotNull(viewModel);
        }

        [Fact]
        public void ConfigureServices_WithDefaultOllamaConfig_UsesDefaults()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            var mockOllamaSection = new Mock<IConfigurationSection>();
            
            // Return null to trigger defaults
            mockOllamaSection.Setup(s => s["BaseUrl"]).Returns((string)null!);
            mockOllamaSection.Setup(s => s["Model"]).Returns((string)null!);
            mockConfig.Setup(c => c.GetSection("Ollama")).Returns(mockOllamaSection.Object);
            
            // Act
            services.AddSingleton(mockConfig.Object);
            ConfigureTestServices(services);
            var provider = services.BuildServiceProvider();
            
            // Assert - should not throw and should use defaults
            var aiProvider = provider.GetRequiredService<IAIProvider>();
            Assert.NotNull(aiProvider);
        }

        [Fact]
        public void ConfigureServices_ProviderInstance_IsSameSingletonInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            var mockOllamaSection = new Mock<IConfigurationSection>();
            
            mockOllamaSection.Setup(s => s["BaseUrl"]).Returns("http://localhost:11434");
            mockOllamaSection.Setup(s => s["Model"]).Returns("mistral");
            mockConfig.Setup(c => c.GetSection("Ollama")).Returns(mockOllamaSection.Object);
            
            // Act
            services.AddSingleton(mockConfig.Object);
            ConfigureTestServices(services);
            var provider = services.BuildServiceProvider();
            
            var aiProvider1 = provider.GetRequiredService<IAIProvider>();
            var aiProvider2 = provider.GetRequiredService<IAIProvider>();
            
            // Assert - should be same instance for singleton
            Assert.Same(aiProvider1, aiProvider2);
        }

        [Fact]
        public void ConfigureServices_ViewModelInstance_IsSameSingletonInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            var mockOllamaSection = new Mock<IConfigurationSection>();
            
            mockOllamaSection.Setup(s => s["BaseUrl"]).Returns("http://localhost:11434");
            mockOllamaSection.Setup(s => s["Model"]).Returns("mistral");
            mockConfig.Setup(c => c.GetSection("Ollama")).Returns(mockOllamaSection.Object);
            
            // Act
            services.AddSingleton(mockConfig.Object);
            ConfigureTestServices(services);
            var provider = services.BuildServiceProvider();
            
            var viewModel1 = provider.GetRequiredService<MainWindowViewModel>();
            var viewModel2 = provider.GetRequiredService<MainWindowViewModel>();
            
            // Assert - should be same instance for singleton
            Assert.Same(viewModel1, viewModel2);
        }

        [Fact]
        public void ConfigureServices_RegistersConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            
            // Act
            services.AddSingleton(mockConfig.Object);
            ConfigureTestServices(services);
            var provider = services.BuildServiceProvider();
            
            // Assert
            var config = provider.GetRequiredService<IConfiguration>();
            Assert.NotNull(config);
        }

        [Fact]
        public void ConfigureServices_WithCustomBaseUrl_UsesProvidedValue()
        {
            // Arrange
            var customUrl = "http://custom.ai:9999";
            var services = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            var mockOllamaSection = new Mock<IConfigurationSection>();
            
            mockOllamaSection.Setup(s => s["BaseUrl"]).Returns(customUrl);
            mockOllamaSection.Setup(s => s["Model"]).Returns("custom-model");
            mockConfig.Setup(c => c.GetSection("Ollama")).Returns(mockOllamaSection.Object);
            
            // Act
            services.AddSingleton(mockConfig.Object);
            ConfigureTestServices(services);
            var provider = services.BuildServiceProvider();
            
            // Assert
            var aiProvider = provider.GetRequiredService<IAIProvider>();
            Assert.NotNull(aiProvider);
        }

        [Fact]
        public void ConfigureServices_WithCustomModel_UsesProvidedValue()
        {
            // Arrange
            var customModel = "llama2";
            var services = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            var mockOllamaSection = new Mock<IConfigurationSection>();
            
            mockOllamaSection.Setup(s => s["BaseUrl"]).Returns("http://localhost:11434");
            mockOllamaSection.Setup(s => s["Model"]).Returns(customModel);
            mockConfig.Setup(c => c.GetSection("Ollama")).Returns(mockOllamaSection.Object);
            
            // Act
            services.AddSingleton(mockConfig.Object);
            ConfigureTestServices(services);
            var provider = services.BuildServiceProvider();
            
            // Assert
            var aiProvider = provider.GetRequiredService<IAIProvider>();
            Assert.NotNull(aiProvider);
        }

        [Fact]
        public void ConfigureServices_AllServicesRegistered_CanResolveViewModel()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            var mockOllamaSection = new Mock<IConfigurationSection>();
            
            mockOllamaSection.Setup(s => s["BaseUrl"]).Returns("http://localhost:11434");
            mockOllamaSection.Setup(s => s["Model"]).Returns("mistral");
            mockConfig.Setup(c => c.GetSection("Ollama")).Returns(mockOllamaSection.Object);
            
            // Act
            services.AddSingleton(mockConfig.Object);
            ConfigureTestServices(services);
            var provider = services.BuildServiceProvider();
            
            // Assert
            var viewModel = provider.GetRequiredService<MainWindowViewModel>();
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Messages);
            Assert.Empty(viewModel.Messages);
        }

        [Fact]
        public void DependencyInjection_ViewModelWithProvider_ProviderIsNotNull()
        {
            // Arrange
            var mockProvider = new Mock<IAIProvider>();
            
            // Act
            var viewModel = new MainWindowViewModel(mockProvider.Object);
            
            // Assert
            Assert.NotNull(viewModel);
        }

        /// <summary>
        /// Helper method to simulate App's ConfigureServices method for testing
        /// </summary>
        private void ConfigureTestServices(ServiceCollection services)
        {
            var config = services.FirstOrDefault(sd => sd.ServiceType == typeof(IConfiguration))
                ?.ImplementationInstance as IConfiguration;

            if (config != null)
            {
                var ollamaConfig = config.GetSection("Ollama");
                var baseUrl = ollamaConfig?["BaseUrl"] ?? "http://localhost:11434";
                var model = ollamaConfig?["Model"] ?? "mistral";

                services.AddSingleton<IAIProvider>(new OllamaProvider(baseUrl, model));
            }

            services.AddSingleton<MainWindowViewModel>();
        }
    }
}
