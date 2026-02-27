using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReserV7.Views.Pages;
using ReserV7.Views.Windows;
using Wpf.Ui;

namespace ReserV7.Services
{
    /// <summary>
    /// Managed host of the application.
    /// </summary>
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthService _authService;
        private readonly DatabaseInitializerService _databaseInitializer;

        private INavigationWindow _navigationWindow;

        public ApplicationHostService(IServiceProvider serviceProvider, IAuthService authService, DatabaseInitializerService databaseInitializer)
        {
            _serviceProvider = serviceProvider;
            _authService = authService;
            _databaseInitializer = databaseInitializer;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Initialisation de la base de données avant de lancer l'application
            await _databaseInitializer.InitializeAsync();
            await HandleActivationAsync();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Affiche la page principale em meme temps
        /// </summary>
        private async Task HandleActivationAsync()
        {
            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                _navigationWindow = (
                    _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow
                )!;
                _navigationWindow!.ShowWindow();

                // Si non authoriser, rediriger vers la page de login, sinon vers le dashboard  
                if (!_authService.IsAuthenticated)
                {
                    _navigationWindow.Navigate(typeof(Views.Pages.LoginPage));
                }
                else
                {
                    _navigationWindow.Navigate(typeof(Views.Pages.DashboardPage));
                }
            }

            await Task.CompletedTask;
        }
    }
}
