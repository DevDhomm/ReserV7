using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Spacium.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            ApplyPatternForTheme(CurrentTheme);
            AppVersion = $"UiDesktopApp1 - {GetAssemblyVersion()}";

            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;
                    ApplyPatternForTheme(CurrentTheme);
                    Application.Current?.Dispatcher.BeginInvoke(
                        () => ApplyPatternForTheme(CurrentTheme),
                        DispatcherPriority.ApplicationIdle
                    );

                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;
                    ApplyPatternForTheme(CurrentTheme);
                    Application.Current?.Dispatcher.BeginInvoke(
                        () => ApplyPatternForTheme(CurrentTheme),
                        DispatcherPriority.ApplicationIdle
                    );

                    break;
            }
        }

        private static void ApplyPatternForTheme(ApplicationTheme theme)
        {
            var resources = Application.Current?.Resources;
            if (resources == null)
                return;

            var resourceKey = theme == ApplicationTheme.Dark
                ? "PagePatternBackgroundBrushDark"
                : "PagePatternBackgroundBrushLight";

            if (resources[resourceKey] is Brush brush)
            {
                resources["PagePatternBackgroundBrush"] = brush.CloneCurrentValue();
            }
        }
    }
}

