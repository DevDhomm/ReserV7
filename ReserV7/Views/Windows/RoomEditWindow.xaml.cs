using ReserV7.Models;
using System.Collections.ObjectModel;
using System.Windows;
using ReserV7.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace ReserV7.Views.Windows
{
    public partial class RoomEditWindow : Window
    {
        private readonly RoomEditViewModel _viewModel;

        public RoomEditWindow(Salle? salle = null)
        {
            InitializeComponent();

            _viewModel = new RoomEditViewModel(salle);
            DataContext = _viewModel;

            if (salle != null)
            {
                Title = $"Éditer : {salle.Nom}";
            }
            else
            {
                Title = "Ajouter : Nouvelle Salle";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class RoomEditViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationDbContext _context;
        private Salle? _currentSalle;

        private string _roomName = string.Empty;
        private string _roomDescription = string.Empty;
        private int _roomCapacity = 0;
        private string _roomType = string.Empty;
        private int _roomFloor = 1;
        private bool _isAvailable = true;

        public string RoomName
        {
            get => _roomName;
            set
            {
                if (_roomName != value)
                {
                    _roomName = value;
                    OnPropertyChanged(nameof(RoomName));
                }
            }
        }

        public string RoomDescription
        {
            get => _roomDescription;
            set
            {
                if (_roomDescription != value)
                {
                    _roomDescription = value;
                    OnPropertyChanged(nameof(RoomDescription));
                }
            }
        }

        public int RoomCapacity
        {
            get => _roomCapacity;
            set
            {
                if (_roomCapacity != value)
                {
                    _roomCapacity = value;
                    OnPropertyChanged(nameof(RoomCapacity));
                }
            }
        }

        public string RoomType
        {
            get => _roomType;
            set
            {
                if (_roomType != value)
                {
                    _roomType = value;
                    OnPropertyChanged(nameof(RoomType));
                }
            }
        }

        public int RoomFloor
        {
            get => _roomFloor;
            set
            {
                if (_roomFloor != value)
                {
                    _roomFloor = value;
                    OnPropertyChanged(nameof(RoomFloor));
                }
            }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                if (_isAvailable != value)
                {
                    _isAvailable = value;
                    OnPropertyChanged(nameof(IsAvailable));
                }
            }
        }

        public ObservableCollection<Equipement> RoomEquipments { get; } = new();

        public RelayCommand SaveCommand { get; }
        public RelayCommand AddEquipmentCommand { get; }
        public RelayCommand<Equipement> EditEquipmentCommand { get; }
        public RelayCommand<Equipement> RemoveEquipmentCommand { get; }

        public RoomEditViewModel(Salle? salle = null)
        {
            _context = App.Services.GetRequiredService<ApplicationDbContext>();
            _currentSalle = salle;

            if (salle != null)
            {
                RoomName = salle.Nom;
                RoomDescription = salle.Description;
                RoomCapacity = salle.Capacite;
                RoomType = salle.Type;
                RoomFloor = salle.Etage;
                IsAvailable = salle.Disponibilite;

                foreach (var equipment in salle.Equipements)
                {
                    RoomEquipments.Add(equipment);
                }
            }

            SaveCommand = new RelayCommand(SaveRoom);
            AddEquipmentCommand = new RelayCommand(AddEquipment);
            EditEquipmentCommand = new RelayCommand<Equipement>(EditEquipment);
            RemoveEquipmentCommand = new RelayCommand<Equipement>(RemoveEquipment);
        }

        private void SaveRoom()
        {
            if (string.IsNullOrWhiteSpace(RoomName))
            {
                MessageBox.Show("Le nom de la salle est requis.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (RoomCapacity <= 0)
            {
                MessageBox.Show("La capacité doit être supérieure à 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_currentSalle == null)
                {
                    var newSalle = new Salle
                    {
                        Nom = RoomName.Trim(),
                        Description = RoomDescription.Trim(),
                        Capacite = RoomCapacity,
                        Type = RoomType,
                        Etage = RoomFloor,
                        Disponibilite = IsAvailable
                    };

                    foreach (var equipment in RoomEquipments)
                    {
                        equipment.SalleId = 0;
                        newSalle.Equipements.Add(equipment);
                    }

                    _context.Salles.Add(newSalle);
                }
                else
                {
                    _currentSalle.Nom = RoomName.Trim();
                    _currentSalle.Description = RoomDescription.Trim();
                    _currentSalle.Capacite = RoomCapacity;
                    _currentSalle.Type = RoomType;
                    _currentSalle.Etage = RoomFloor;
                    _currentSalle.Disponibilite = IsAvailable;

                    var existingEquipmentIds = _currentSalle.Equipements.Select(e => e.Id).ToList();
                    var newEquipmentIds = RoomEquipments.Select(e => e.Id).ToList();
                    var equipmentToRemove = _currentSalle.Equipements
                        .Where(e => !newEquipmentIds.Contains(e.Id))
                        .ToList();

                    foreach (var equipment in equipmentToRemove)
                    {
                        _currentSalle.Equipements.Remove(equipment);
                        if (equipment.Id > 0)
                        {
                            _context.Equipements.Remove(equipment);
                        }
                    }

                    foreach (var equipment in RoomEquipments)
                    {
                        if (equipment.Id == 0)
                        {
                            equipment.SalleId = _currentSalle.Id;
                            _currentSalle.Equipements.Add(equipment);
                        }
                    }
                }

                _context.SaveChanges();
                MessageBox.Show("Salle sauvegardée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                var window = Application.Current.Windows.OfType<RoomEditWindow>().FirstOrDefault();
                window?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddEquipment()
        {
            var editWindow = new EquipmentEditWindow();
            if (editWindow.ShowDialog() == true && editWindow.SavedEquipment != null)
            {
                RoomEquipments.Add(editWindow.SavedEquipment);
            }
        }

        private void EditEquipment(Equipement? equipment)
        {
            if (equipment == null)
                return;

            var editWindow = new EquipmentEditWindow(equipment);
            editWindow.ShowDialog();
        }

        private void RemoveEquipment(Equipement? equipment)
        {
            if (equipment == null)
                return;

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer '{equipment.Nom}' ?",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RoomEquipments.Remove(equipment);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

        public void Execute(object? parameter) => _execute((T?)parameter);
    }
}
