using System.Windows;
using System.Windows.Controls;

namespace ReserV7.Views.Windows
{
    public partial class UserEditWindow : Window
    {
        public UserEditWindow()
        {
            InitializeComponent();

            // Handle password box binding
            this.Loaded += (s, e) =>
            {
                var passwordBox = this.FindName("PasswordBox") as PasswordBox;
                if (passwordBox != null && DataContext is IUserEditWindowViewModel viewModel)
                {
                    passwordBox.PasswordChanged += (sender, args) =>
                    {
                        viewModel.UserPassword = passwordBox.Password;
                    };
                }
            };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public interface IUserEditWindowViewModel
    {
        string UserNom { get; set; }
        string UserEmail { get; set; }
        string UserUsername { get; set; }
        string UserPassword { get; set; }
        string UserRole { get; set; }
    }
}
