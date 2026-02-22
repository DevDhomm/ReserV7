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
        private ObservableCollection<SalleDisplayModel> filteredSalles = new();

        [ObservableProperty]
        private string searchText = string.Empty;

        public RoomsViewModel(ApplicationDbContext context)
        {
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            var rooms = _context.Salles
                .Include(s => s.Equipements)
                .OrderBy(s => s.Nom)
                .ToList();

            var displayModels = rooms.Select(s => new SalleDisplayModel(s)).ToList();
            FilteredSalles = new ObservableCollection<SalleDisplayModel>(displayModels);
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            var searchTerm = SearchText.ToLower();

            var allRooms = _context.Salles
                .Include(s => s.Equipements)
                .OrderBy(s => s.Nom)
                .ToList();

            var filtered = allRooms
                .Where(s => 
                    s.Nom.ToLower().Contains(searchTerm) ||
                    s.Description.ToLower().Contains(searchTerm) ||
                    s.Type.ToLower().Contains(searchTerm) ||
                    s.Equipements.Any(e => e.Nom.ToLower().Contains(searchTerm)))
                .Select(s => new SalleDisplayModel(s))
                .ToList();

            FilteredSalles = new ObservableCollection<SalleDisplayModel>(filtered);
        }

        [RelayCommand]
        private void Search()
        {
            PerformSearch();
        }

        [RelayCommand]
        private void AddRoom()
        {
            // Open add room dialog window
            var editWindow = new Views.Windows.RoomEditWindow();
            editWindow.ShowDialog();
            LoadData();
        }

        [RelayCommand]
        private void EditRoom(SalleDisplayModel? room)
        {
            if (room?.Salle == null)
                return;

            var editWindow = new Views.Windows.RoomEditWindow(room.Salle);
            editWindow.ShowDialog();
            LoadData();
        }

        [RelayCommand]
        private void DeleteRoom(SalleDisplayModel? room)
        {
            if (room?.Salle == null)
                return;

            _context.Salles.Remove(room.Salle);
            _context.SaveChanges();
            FilteredSalles.Remove(room);
        }

        partial void OnSearchTextChanged(string value)
        {
            PerformSearch();
        }
    }

    public class SalleDisplayModel
    {
        public Salle Salle { get; }
        public int EquipementCount => Salle.Equipements.Count;

        public int Id => Salle.Id;
        public string Nom => Salle.Nom;
        public int Capacite => Salle.Capacite;
        public string Type => Salle.Type;
        public int Etage => Salle.Etage;
        public bool Disponibilite => Salle.Disponibilite;

        public SalleDisplayModel(Salle salle)
        {
            Salle = salle;
        }
    }
}
