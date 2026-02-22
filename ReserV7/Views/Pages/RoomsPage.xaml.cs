using ReserV7.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Controls;

namespace ReserV7.Views.Pages
{
    public partial class RoomsPage : Page, INavigableView<RoomsViewModel>
    {
        public RoomsViewModel ViewModel { get; }

        public RoomsPage(RoomsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
