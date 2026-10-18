using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ntech.AI.Core.Interfaces;
using Ntech.AI.Core.Providers;

namespace Ntech.AI.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }
        public IConfiguration? Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load configuration
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = configBuilder.Build();

            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            MainWindow = new MainWindow { DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>() };
            MainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Add configuration
            services.AddSingleton(Configuration);

            // Add Ollama provider
            var ollamaConfig = Configuration?.GetSection("Ollama");
            var baseUrl = ollamaConfig?["BaseUrl"] ?? "http://localhost:11434";
            var model = ollamaConfig?["Model"] ?? "mistral";

            services.AddSingleton<IAIProvider>(new OllamaProvider(baseUrl, model));

            // Add ViewModels
            services.AddSingleton<MainWindowViewModel>();
        }
    }
}