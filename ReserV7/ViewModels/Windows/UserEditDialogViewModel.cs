using ReserV7.Models;
using ReserV7.Views.Windows;

namespace ReserV7.ViewModels.Windows
{
    public partial class UserEditDialogViewModel : ObservableObject, IUserEditWindowViewModel
    {
        private string _userNom = string.Empty;
        private string _userEmail = string.Empty;
        private string _userUsername = string.Empty;
        private string _userPassword = string.Empty;
        private string _userRole = "User";

        public string UserNom
        {
            get => _userNom;
            set => SetProperty(ref _userNom, value);
        }

        public string UserEmail
        {
            get => _userEmail;
            set => SetProperty(ref _userEmail, value);
        }

        public string UserUsername
        {
            get => _userUsername;
            set => SetProperty(ref _userUsername, value);
        }

        public string UserPassword
        {
            get => _userPassword;
            set => SetProperty(ref _userPassword, value);
        }

        public string UserRole
        {
            get => _userRole;
            set => SetProperty(ref _userRole, value);
        }

        public UserEditDialogViewModel(User? user)
        {
            if (user != null)
            {
                _userNom = user.Nom;
                _userEmail = user.Email;
                _userUsername = user.Username;
                _userRole = user.Role;
                _userPassword = string.Empty; // Don't show existing password
            }
        }
    }
}
