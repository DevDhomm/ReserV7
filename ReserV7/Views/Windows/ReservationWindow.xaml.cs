using ReserV7.ViewModels.Pages;
using System.Windows;

namespace ReserV7.Views.Windows
{
    public partial class ReservationWindow : Window
    {
        public BookRoomViewModel? ViewModel { get; set; }

        public ReservationWindow(BookRoomViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            // Update header with room name
            if (viewModel.SelectedSalle != null)
            {
                SalleNameBlock.Text = $"Salle: {viewModel.SelectedSalle.Nom}";
            }

            // Show message if exists
            if (!string.IsNullOrEmpty(viewModel.AvailabilityMessage))
            {
                // Message will be displayed in the form if needed
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
