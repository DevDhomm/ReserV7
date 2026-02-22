using ReserV7.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Controls;

namespace ReserV7.Views.Pages
{
    public partial class ReservationPage : Page, INavigableView<ReservationViewModel>
    {
        public ReservationViewModel ViewModel { get; }

        public ReservationPage(ReservationViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            // Refresh data when page is loaded
            this.Loaded += (s, e) => ViewModel.RefreshReservationsCommand.Execute(null);
        }
    }
}
