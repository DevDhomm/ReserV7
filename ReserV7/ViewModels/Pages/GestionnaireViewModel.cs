using Microsoft.EntityFrameworkCore;
using ReserV7.Data;
using ReserV7.Models;
using ReserV7.ViewModels.Windows;
using ReserV7.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows;

namespace ReserV7.ViewModels.Pages
{
    public partial class GestionnaireViewModel : ObservableObject
    {
        private readonly ApplicationDbContext _context;

        [ObservableProperty]
        private ObservableCollection<User> utilisateurs = new();

        [ObservableProperty]
        private ObservableCollection<User> filteredUtilisateurs = new();

        [ObservableProperty]
        private User? selectedUtilisateur;

        [ObservableProperty]
        private int totalUtilisateurs = 0;

        [ObservableProperty]
        private string searchText = string.Empty;

        private User? _editingUser = null;

        public GestionnaireViewModel(ApplicationDbContext context)
        {
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            // Load all users
            var users = _context.Users.ToList();
            Utilisateurs = new ObservableCollection<User>(users);
            ApplyFilter();

            // Calculate statistics
            TotalUtilisateurs = users.Count;
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                var userList = Utilisateurs.ToList();
                FilteredUtilisateurs = new ObservableCollection<User>(userList);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = Utilisateurs
                    .Where(u => u.Nom.ToLower().Contains(searchLower) ||
                                u.Email.ToLower().Contains(searchLower) ||
                                u.Username.ToLower().Contains(searchLower))
                    .ToList();
                FilteredUtilisateurs = new ObservableCollection<User>(filtered);
            }
        }

        [RelayCommand]
        private void Search()
        {
            ApplyFilter();
        }

        // User Management Commands

        [RelayCommand]
        private void EditUtilisateur(User? user)
        {
            if (user == null)
                return;

            _editingUser = user;

            // Create a view model for the dialog
            var dialogViewModel = new UserEditDialogViewModel(user);

            // Create and show the dialog
            var window = new UserEditWindow
            {
                DataContext = dialogViewModel,
                Title = $"Éditer : {user.Nom}"
            };

            // Update title to show it's editing
            var titleBlock = window.FindName("TitleBlock") as System.Windows.Controls.TextBlock;
            if (titleBlock != null)
            {
                titleBlock.Text = $"Éditer : {user.Nom}";
            }

            if (window.ShowDialog() == true)
            {
                // Update the user in database
                user.Nom = dialogViewModel.UserNom;
                user.Email = dialogViewModel.UserEmail;
                user.Username = dialogViewModel.UserUsername;
                user.Role = dialogViewModel.UserRole;

                // Only update password if a new one was provided
                if (!string.IsNullOrWhiteSpace(dialogViewModel.UserPassword))
                {
                    user.Password = dialogViewModel.UserPassword;
                }

                _context.Users.Update(user);
                _context.SaveChanges();
                LoadData();
            }

            _editingUser = null;
        }

        [RelayCommand]
        private void AddUtilisateur()
        {
            // Create a view model for the dialog with empty user
            var dialogViewModel = new UserEditDialogViewModel(null);

            // Create and show the dialog
            var window = new UserEditWindow
            {
                DataContext = dialogViewModel,
                Title = "Ajouter : Nouvel Utilisateur"
            };

            // Update title to show it's adding
            var titleBlock = window.FindName("TitleBlock") as System.Windows.Controls.TextBlock;
            if (titleBlock != null)
            {
                titleBlock.Text = "Ajouter : Nouvel Utilisateur";
            }

            if (window.ShowDialog() == true)
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(dialogViewModel.UserNom) ||
                    string.IsNullOrWhiteSpace(dialogViewModel.UserUsername) ||
                    string.IsNullOrWhiteSpace(dialogViewModel.UserPassword))
                {
                    MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if username already exists
                if (_context.Users.Any(u => u.Username == dialogViewModel.UserUsername))
                {
                    MessageBox.Show("Ce nom d'utilisateur existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create and save the new user
                var newUser = new User
                {
                    Nom = dialogViewModel.UserNom,
                    Email = dialogViewModel.UserEmail,
                    Username = dialogViewModel.UserUsername,
                    Password = dialogViewModel.UserPassword,
                    Role = dialogViewModel.UserRole,
                    DateCreation = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();
                LoadData();

                MessageBox.Show("Utilisateur créé avec succès!", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void DeleteUtilisateur(User? user)
        {
            if (user == null)
                return;

            _context.Users.Remove(user);
            _context.SaveChanges();
            LoadData();
        }
    }
}
