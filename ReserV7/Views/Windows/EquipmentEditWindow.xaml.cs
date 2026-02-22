using ReserV7.Models;
using System.ComponentModel;
using System.Windows;

namespace ReserV7.Views.Windows
{
    public partial class EquipmentEditWindow : Window
    {
        private readonly EquipmentEditViewModel _viewModel;
        public Equipement? SavedEquipment => _viewModel.GetEquipment();

        public EquipmentEditWindow(Equipement? equipment = null)
        {
            InitializeComponent();

            _viewModel = new EquipmentEditViewModel(equipment, this);
            DataContext = _viewModel;

            if (equipment != null)
            {
                Title = "Éditer Équipement";
                TitleBlock.Text = "Éditer Équipement";
            }
            else
            {
                Title = "Ajouter Équipement";
                TitleBlock.Text = "Ajouter Équipement";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    public class EquipmentEditViewModel : INotifyPropertyChanged
    {
        private Equipement? _currentEquipment;
        private readonly Window _window;
        private string _equipmentName = string.Empty;
        private string _equipmentType = string.Empty;
        private string _equipmentDescription = string.Empty;
        private bool _isEquipmentFunctional = true;

        public string EquipmentName
        {
            get => _equipmentName;
            set
            {
                if (_equipmentName != value)
                {
                    _equipmentName = value;
                    OnPropertyChanged(nameof(EquipmentName));
                }
            }
        }

        public string EquipmentType
        {
            get => _equipmentType;
            set
            {
                if (_equipmentType != value)
                {
                    _equipmentType = value;
                    OnPropertyChanged(nameof(EquipmentType));
                }
            }
        }

        public string EquipmentDescription
        {
            get => _equipmentDescription;
            set
            {
                if (_equipmentDescription != value)
                {
                    _equipmentDescription = value;
                    OnPropertyChanged(nameof(EquipmentDescription));
                }
            }
        }

        public bool IsEquipmentFunctional
        {
            get => _isEquipmentFunctional;
            set
            {
                if (_isEquipmentFunctional != value)
                {
                    _isEquipmentFunctional = value;
                    OnPropertyChanged(nameof(IsEquipmentFunctional));
                }
            }
        }

        public RelayCommand SaveCommand { get; }

        public EquipmentEditViewModel(Equipement? equipment = null, Window? window = null)
        {
            _currentEquipment = equipment;
            _window = window ?? Application.Current.MainWindow!;

            if (equipment != null)
            {
                EquipmentName = equipment.Nom;
                EquipmentType = equipment.Type;
                EquipmentDescription = equipment.Description;
                IsEquipmentFunctional = equipment.EstFonctionnel;
            }

            SaveCommand = new RelayCommand(SaveEquipment);
        }

        private void SaveEquipment()
        {
            if (string.IsNullOrWhiteSpace(EquipmentName))
            {
                MessageBox.Show("Le nom de l'équipement est requis.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(EquipmentType))
            {
                MessageBox.Show("Le type d'équipement est requis.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_currentEquipment == null)
                {
                    _currentEquipment = new Equipement
                    {
                        Nom = EquipmentName.Trim(),
                        Type = EquipmentType,
                        Description = EquipmentDescription.Trim(),
                        EstFonctionnel = IsEquipmentFunctional
                    };
                }
                else
                {
                    _currentEquipment.Nom = EquipmentName.Trim();
                    _currentEquipment.Type = EquipmentType;
                    _currentEquipment.Description = EquipmentDescription.Trim();
                    _currentEquipment.EstFonctionnel = IsEquipmentFunctional;
                }

                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public Equipement? GetEquipment() => _currentEquipment;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

