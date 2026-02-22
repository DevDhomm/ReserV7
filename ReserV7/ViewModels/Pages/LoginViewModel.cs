using System.Windows.Input;
using Wpf.Ui;

namespace ReserV7.ViewModels.Pages
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly Services.IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        public LoginViewModel(Services.IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void Login()
        {
            if (_authService.Login(Username, Password))
            {
                // Navigate to dashboard after successful login
                _navigationService.Navigate(typeof(Views.Pages.DashboardPage));
            }
            else
            {
                // Failed login: simple no-op for demo. Real app show message.
            }
        }
    }
}
