using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spacium.Data;
using Spacium.Services;
using Spacium.ViewModels.Pages;
using Spacium.ViewModels.Windows;
using Spacium.Views.Pages;
using Spacium.Views.Windows;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.DependencyInjection;

namespace Spacium
    {
        /// <summary>
        /// Interaction logic for App.xaml
        /// </summary>
        public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)); })
            .ConfigureServices((context, services) =>
            {
                // Database
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Spacium",
                    "app.db"
                );
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));

                services.AddSingleton<DatabaseInitializerService>();

                services.AddNavigationViewPageProvider();

                services.AddHostedService<ApplicationHostService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<DashboardPage>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();

                // Reservation system pages
                services.AddSingleton<ReservationPage>();
                services.AddSingleton<ReservationViewModel>();
                services.AddSingleton<BookRoomPage>();
                services.AddSingleton<BookRoomViewModel>();
                services.AddSingleton<RoomsPage>();
                services.AddSingleton<RoomsViewModel>();
                services.AddSingleton<GestionnairePage>();
                services.AddSingleton<GestionnaireViewModel>();

                // Auth and login
                services.AddSingleton<IAuthService, AuthService>();
                services.AddSingleton<LoginPage>();
                services.AddSingleton<LoginViewModel>();
            }).Build();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get { return _host.Services; }
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            ApplyPatternForCurrentTheme();

            // Initialize database
            var dbInitializer = _host.Services.GetRequiredService<DatabaseInitializerService>();
            await dbInitializer.InitializeAsync();

            await _host.StartAsync();
        }

        private void ApplyPatternForCurrentTheme()
        {
            var currentTheme = ApplicationThemeManager.GetAppTheme();
            var key = currentTheme == ApplicationTheme.Dark
                ? "PagePatternBackgroundBrushDark"
                : "PagePatternBackgroundBrushLight";

            if (Resources[key] is System.Windows.Media.Brush brush)
            {
                Resources["PagePatternBackgroundBrush"] = brush.CloneCurrentValue();
            }
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}

