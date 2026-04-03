using Spacium.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spacium.Views.Pages
{
    public partial class ReservationPage : Page, INavigableView<ReservationViewModel>
    {
        public ReservationViewModel ViewModel { get; }

        public ReservationPage(ReservationViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            AddHandler(MouseWheelEvent, new MouseWheelEventHandler(OnPageMouseWheel), true);

            // Refresh data when page is loaded
            this.Loaded += (s, e) => ViewModel.RefreshReservationsCommand.Execute(null);
        }

        private void OnPageMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ReservationsScrollViewer is not null && ReservationsScrollViewer.IsMouseOver && ReservationsScrollViewer.ScrollableHeight > 0)
            {
                ReservationsScrollViewer.ScrollToVerticalOffset(ReservationsScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void ReservationsScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }
}

