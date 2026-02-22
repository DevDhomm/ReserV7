using ReserV7.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Controls;

namespace ReserV7.Views.Pages
{
    public partial class BookRoomPage : Page, INavigableView<BookRoomViewModel>
    {
        public BookRoomViewModel ViewModel { get; }

        public BookRoomPage(BookRoomViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
