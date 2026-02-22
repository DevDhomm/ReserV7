using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace ReserV7.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly Services.IAuthService _authService;

        public MainWindowViewModel(Services.IAuthService authService)
        {
            _authService = authService;
            _authService.UserChanged += OnUserChanged;

            UpdateMenu();
            UpdateFooter();
        }

        private void OnUserChanged()
        {
            UpdateMenu();
            UpdateFooter();
        }

        private void UpdateMenu()
        {
            var items = new ObservableCollection<object>();

            // Home page - accessible to everyone
            items.Add(new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            });

            // Check authentication and role
            if (_authService.IsAuthenticated)
            {
                var role = _authService.CurrentRole;

                if (role == "User")
                {
                    // User: Réserver une salle + Historique
                    items.Add(new NavigationViewItem()
                    {
                        Content = "Réserver une Salle",
                        Icon = new SymbolIcon { Symbol = SymbolRegular.CalendarAdd24 },
                        TargetPageType = typeof(Views.Pages.BookRoomPage)
                    });

                    items.Add(new NavigationViewItem()
                    {
                        Content = "Mes Réservations",
                        Icon = new SymbolIcon { Symbol = SymbolRegular.CalendarLtr24 },
                        TargetPageType = typeof(Views.Pages.ReservationPage)
                    });
                }
                else if (role == "Gestionnaire")
                {
                    // Gestionnaire: full access
                    items.Add(new NavigationViewItem()
                    {
                        Content = "Réserver une Salle",
                        Icon = new SymbolIcon { Symbol = SymbolRegular.CalendarAdd24 },
                        TargetPageType = typeof(Views.Pages.BookRoomPage)
                    });

                    items.Add(new NavigationViewItem()
                    {
                        Content = "Réservations",
                        Icon = new SymbolIcon { Symbol = SymbolRegular.CalendarLtr24 },
                        TargetPageType = typeof(Views.Pages.ReservationPage)
                    });

                    items.Add(new NavigationViewItem()
                    {
                        Content = "Salles",
                        Icon = new SymbolIcon { Symbol = SymbolRegular.BuildingBank24 },
                        TargetPageType = typeof(Views.Pages.RoomsPage)
                    });

                    items.Add(new NavigationViewItem()
                    {
                        Content = "Gestionnaire",
                        Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                        TargetPageType = typeof(Views.Pages.GestionnairePage)
                    });
                }
            }

            _menuItems = items;
            OnPropertyChanged(nameof(MenuItems));
        }

        private void UpdateFooter()
        {
            var footer = new ObservableCollection<object>();

            if (_authService.IsAuthenticated)
            {
                footer.Add(new NavigationViewItem()
                {
                    Content = "Settings",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                    TargetPageType = typeof(Views.Pages.SettingsPage)
                });
            }
            else
            {
                footer.Add(new NavigationViewItem()
                {
                    Content = "Login",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Person24 },
                    TargetPageType = typeof(Views.Pages.LoginPage)
                });
            }

            _footerMenuItems = footer;
            OnPropertyChanged(nameof(FooterMenuItems));
        }

        [ObservableProperty]
        private string _applicationTitle = "WPF UI - ReserV7";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new();

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
