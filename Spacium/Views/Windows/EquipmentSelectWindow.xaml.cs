using Spacium.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Spacium.Views.Windows
{
    public partial class EquipmentSelectWindow : Window
    {
        private readonly EquipmentSelectViewModel _viewModel;

        public Equipement? SelectedEquipment => _viewModel.SelectedExistingEquipment;
        public Equipement? NewEquipment => _viewModel.NewEquipment;

        public EquipmentSelectWindow(List<Equipement> availableEquipment)
        {
            InitializeComponent();

            _viewModel = new EquipmentSelectViewModel(availableEquipment);
            DataContext = _viewModel;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Determine which tab is selected
            var selectedTab = ((TabControl)this.FindName("TabControl")).SelectedIndex;

            if (selectedTab == 0) // Select existing tab
            {
                if (_viewModel.SelectedExistingEquipment == null)
                {
                    MessageBox.Show("Veuillez sélectionner un équipement.", "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else // Create new tab
            {
                var nameBox = (TextBox)this.FindName("NewEquipmentName");
                var typeBox = (TextBox)this.FindName("NewEquipmentType");
                var descriptionBox = (TextBox)this.FindName("NewEquipmentDescription");

                if (string.IsNullOrWhiteSpace(nameBox?.Text))
                {
                    MessageBox.Show("Le nom de l'équipement est requis.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(typeBox?.Text))
                {
                    MessageBox.Show("Le type d'équipement est requis.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _viewModel.CreateNewEquipment(nameBox.Text, typeBox.Text, descriptionBox?.Text ?? string.Empty);
            }

            this.DialogResult = true;
            this.Close();
        }
    }

    public class EquipmentSelectViewModel : INotifyPropertyChanged
    {
        private List<Equipement> _allEquipment;
        private string _searchText = string.Empty;
        private Equipement? _selectedExistingEquipment;
        public Equipement? NewEquipment { get; private set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    UpdateFilteredEquipment();
                }
            }
        }

        public Equipement? SelectedExistingEquipment
        {
            get => _selectedExistingEquipment;
            set
            {
                if (_selectedExistingEquipment != value)
                {
                    _selectedExistingEquipment = value;
                    OnPropertyChanged(nameof(SelectedExistingEquipment));
                }
            }
        }

        private ObservableCollection<Equipement> _filteredEquipment;

        public ObservableCollection<Equipement> FilteredEquipment
        {
            get => _filteredEquipment;
            private set
            {
                if (_filteredEquipment != value)
                {
                    _filteredEquipment = value;
                    OnPropertyChanged(nameof(FilteredEquipment));
                }
            }
        }

        public EquipmentSelectViewModel(List<Equipement> availableEquipment)
        {
            _allEquipment = availableEquipment ?? new List<Equipement>();
            _filteredEquipment = new ObservableCollection<Equipement>(_allEquipment);
        }

        private void UpdateFilteredEquipment()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredEquipment = new ObservableCollection<Equipement>(_allEquipment);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = _allEquipment
                    .Where(e => e.Nom.ToLower().Contains(searchLower) ||
                                e.Type.ToLower().Contains(searchLower) ||
                                e.Description.ToLower().Contains(searchLower))
                    .ToList();
                FilteredEquipment = new ObservableCollection<Equipement>(filtered);
            }
        }

        public void CreateNewEquipment(string name, string type, string description)
        {
            NewEquipment = new Equipement
            {
                Nom = name.Trim(),
                Type = type.Trim(),
                Description = description.Trim(),
                EstFonctionnel = true
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
