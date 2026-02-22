using Microsoft.EntityFrameworkCore;
using ReserV7.Data;
using ReserV7.Models;
using System.Collections.ObjectModel;

namespace ReserV7.ViewModels.Pages
{
    public partial class GestionnaireViewModel : ObservableObject
    {
        private readonly ApplicationDbContext _context;

        [ObservableProperty]
        private ObservableCollection<Reservation> reservations = new();

        [ObservableProperty]
        private ObservableCollection<User> utilisateurs = new();

        [ObservableProperty]
        private Reservation? selectedReservation;

        [ObservableProperty]
        private string selectedStatut = "Confirmée";

        [ObservableProperty]
        private int totalReservations = 0;

        [ObservableProperty]
        private int reservationsEnAttente = 0;

        [ObservableProperty]
        private int totalUtilisateurs = 0;

        public GestionnaireViewModel(ApplicationDbContext context)
        {
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            // Load all reservations
            var reservs = _context.Reservations
                .Include(r => r.Salle)
                .Include(r => r.User)
                .OrderByDescending(r => r.DateReservation)
                .ToList();
            Reservations = new ObservableCollection<Reservation>(reservs);

            // Load all users
            var users = _context.Users.ToList();
            Utilisateurs = new ObservableCollection<User>(users);

            // Calculate statistics
            TotalReservations = reservs.Count;
            ReservationsEnAttente = reservs.Count(r => r.Statut == "En attente");
            TotalUtilisateurs = users.Count;
        }

        [RelayCommand]
        private void SelectReservation()
        {
            if (SelectedReservation != null)
            {
                SelectedStatut = SelectedReservation.Statut;
            }
        }

        [RelayCommand]
        private void UpdateReservationStatut()
        {
            if (SelectedReservation == null)
                return;

            SelectedReservation.Statut = SelectedStatut;
            _context.Reservations.Update(SelectedReservation);
            _context.SaveChanges();
        }

        [RelayCommand]
        private void AnnulerReservation()
        {
            if (SelectedReservation == null)
                return;

            SelectedReservation.Statut = "Annulée";
            _context.Reservations.Update(SelectedReservation);
            _context.SaveChanges();
        }

        [RelayCommand]
        private void RefreshData()
        {
            LoadData();
        }

        [RelayCommand]
        private void ExportReservations()
        {
            // Demo export function - can be expanded to export to CSV/Excel
            System.Diagnostics.Debug.WriteLine($"Export {TotalReservations} reservations");
        }
    }
}
