using Microsoft.EntityFrameworkCore;
using ReserV7.Data;
using ReserV7.Models;
using System.Collections.ObjectModel;

namespace ReserV7.ViewModels.Pages
{
    public partial class RoomsViewModel : ObservableObject
    {
        private readonly ApplicationDbContext _context;

        [ObservableProperty]
        private ObservableCollection<Salle> salles = new();

        [ObservableProperty]
        private Salle? selectedSalle;

        [ObservableProperty]
        private ObservableCollection<Equipement> selectedEquipements = new();

        [ObservableProperty]
        private string selectedSalleName = string.Empty;

        [ObservableProperty]
        private int selectedSalleCapacite = 0;

        [ObservableProperty]
        private string selectedSalleType = string.Empty;

        [ObservableProperty]
        private int selectedSalleEtage = 1;

        public RoomsViewModel(ApplicationDbContext context)
        {
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            var rooms = _context.Salles
                .Include(s => s.Equipements)
                .ToList();
            Salles = new ObservableCollection<Salle>(rooms);
        }

        [RelayCommand]
        private void SelectionChanged()
        {
            if (SelectedSalle != null)
            {
                SelectedSalleName = SelectedSalle.Nom;
                SelectedSalleCapacite = SelectedSalle.Capacite;
                SelectedSalleType = SelectedSalle.Type;
                SelectedSalleEtage = SelectedSalle.Etage;
                SelectedEquipements = new ObservableCollection<Equipement>(SelectedSalle.Equipements);
            }
            else
            {
                SelectedEquipements.Clear();
            }
        }

        [RelayCommand]
        private void AddRoom()
        {
            if (string.IsNullOrWhiteSpace(SelectedSalleName))
                return;

            var salle = new Salle
            {
                Nom = SelectedSalleName,
                Capacite = SelectedSalleCapacite,
                Type = SelectedSalleType,
                Etage = SelectedSalleEtage,
                Disponibilite = true
            };

            _context.Salles.Add(salle);
            _context.SaveChanges();
            Salles.Add(salle);

            SelectedSalleName = string.Empty;
            SelectedSalleCapacite = 0;
            SelectedSalleType = string.Empty;
        }

        [RelayCommand]
        private void DeleteRoom()
        {
            if (SelectedSalle == null)
                return;

            _context.Salles.Remove(SelectedSalle);
            _context.SaveChanges();
            Salles.Remove(SelectedSalle);
            SelectedSalle = null;
        }

        [RelayCommand]
        private void RefreshRooms()
        {
            LoadData();
        }
    }
}
