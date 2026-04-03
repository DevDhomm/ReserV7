using Spacium.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spacium.Views.Pages
{
    public partial class BookRoomPage : Page, INavigableView<BookRoomViewModel>
    {
        public BookRoomViewModel ViewModel { get; }

        public BookRoomPage(BookRoomViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            AddHandler(MouseWheelEvent, new MouseWheelEventHandler(OnPageMouseWheel), true);
        }

        private void OnPageMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (RoomsScrollViewer is not null && RoomsScrollViewer.IsMouseOver && RoomsScrollViewer.ScrollableHeight > 0)
            {
                RoomsScrollViewer.ScrollToVerticalOffset(RoomsScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void RoomsScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }
}

