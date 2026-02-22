using Microsoft.EntityFrameworkCore;
using ReserV7.Data;
using ReserV7.Models;
using ReserV7.Services;
using ReserV7.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows;

namespace ReserV7.ViewModels.Pages
{
    public partial class ReservationViewModel : ObservableObject
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private List<Reservation> _allReservations = new();

        [ObservableProperty]
        private ObservableCollection<Reservation> reservations = new();

        [ObservableProperty]
        private Reservation? selectedReservation;

        // Filter properties
        [ObservableProperty]
        private string selectedStatut = "Tous";

        [ObservableProperty]
        private string selectedSalle = "";

        [ObservableProperty]
        private string selectedMotif = "";

        [ObservableProperty]
        private ObservableCollection<string> availableStatuts = new();

        [ObservableProperty]
        private ObservableCollection<string> availableSalles = new();

        public ReservationViewModel(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
            LoadData();
        }

        private void LoadData()
        {
            if (_authService.CurrentRole == "User")
            {
                // User only sees their own reservations
                var currentUserName = _authService.CurrentUser ?? "";
                var user = _context.Users.FirstOrDefault(u => u.Username == currentUserName);

                if (user != null)
                {
                    _allReservations = _context.Reservations
                        .Where(r => r.UserId == user.Id)
                        .Include(r => r.Salle)
                        .ThenInclude(s => s.Equipements)
                        .Include(r => r.User)
                        .OrderByDescending(r => r.DateReservation)
                        .ToList();
                }
                else
                {
                    _allReservations = new();
                }
            }
            else
            {
                // Gestionnaire sees all reservations
                _allReservations = _context.Reservations
                    .Include(r => r.Salle)
                    .ThenInclude(s => s.Equipements)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.DateReservation)
                    .ToList();
            }

            // Update reservation statuses based on current time
            UpdateReservationStatuses();

            // Initialize filter options
            PopulateFilterOptions();
            ApplyFilters();
        }

        private void PopulateFilterOptions()
        {
            // Populate available statuts
            var statuts = new List<string> { "Tous" };
            statuts.AddRange(_allReservations
                .Select(r => r.Statut)
                .Distinct()
                .OrderBy(s => s));
            AvailableStatuts = new ObservableCollection<string>(statuts);

            // Populate available salles
            var salles = new List<string> { "" };
            salles.AddRange(_allReservations
                .Where(r => r.Salle != null)
                .Select(r => r.Salle.Nom)
                .Distinct()
                .OrderBy(s => s));
            AvailableSalles = new ObservableCollection<string>(salles);
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            var filtered = _allReservations.AsEnumerable();

            // Filter by statut
            if (!string.IsNullOrEmpty(SelectedStatut) && SelectedStatut != "Tous")
            {
                filtered = filtered.Where(r => r.Statut == SelectedStatut);
            }

            // Filter by salle
            if (!string.IsNullOrEmpty(SelectedSalle))
            {
                filtered = filtered.Where(r => r.Salle?.Nom == SelectedSalle);
            }

            // Filter by motif
            if (!string.IsNullOrEmpty(SelectedMotif))
            {
                filtered = filtered.Where(r => r.Motif.Contains(SelectedMotif, StringComparison.OrdinalIgnoreCase));
            }

            Reservations = new ObservableCollection<Reservation>(filtered.ToList());
        }

        [RelayCommand]
        private void ResetFilters()
        {
            SelectedStatut = "Tous";
            SelectedSalle = "";
            SelectedMotif = "";
            ApplyFilters();
        }

        [RelayCommand]
        private void DeleteReservation()
        {
            if (SelectedReservation == null)
                return;

            _context.Reservations.Remove(SelectedReservation);
            _context.SaveChanges();
            _allReservations.Remove(SelectedReservation);
            ApplyFilters();
            SelectedReservation = null;
        }

        /// <summary>
        /// Updates reservation statuses based on current date and time.
        /// Transitions: En attente → En cours → Terminée (preserves Annulée if explicitly set)
        /// </summary>
        private void UpdateReservationStatuses()
        {
            var now = DateTime.Now;
            bool hasChanges = false;

            foreach (var reservation in _allReservations)
            {
                // Don't update if already cancelled
                if (reservation.Statut == "Annulée")
                    continue;

                try
                {
                    // Parse dates and times
                    if (!DateTime.TryParse(reservation.DateDebut, out DateTime reservationStart))
                        continue;
                    if (!DateTime.TryParse(reservation.DateFin, out DateTime reservationEnd))
                        continue;
                    if (!TimeOnly.TryParse(reservation.HeureDebut, out TimeOnly startTime))
                        continue;
                    if (!TimeOnly.TryParse(reservation.HeureFin, out TimeOnly endTime))
                        continue;

                    // Combine date and time
                    DateTime startDateTime = reservationStart.Date.Add(startTime.ToTimeSpan());
                    DateTime endDateTime = reservationEnd.Date.Add(endTime.ToTimeSpan());

                    string newStatut = reservation.Statut;

                    // Determine new status
                    if (now < startDateTime)
                    {
                        newStatut = "En attente";
                    }
                    else if (now >= startDateTime && now <= endDateTime)
                    {
                        newStatut = "En cours";
                    }
                    else if (now > endDateTime)
                    {
                        newStatut = "Terminée";
                    }

                    // Update if changed
                    if (reservation.Statut != newStatut)
                    {
                        reservation.Statut = newStatut;
                        hasChanges = true;
                    }
                }
                catch
                {
                    // Skip reservations with invalid date/time formats
                    continue;
                }
            }

            // Save changes if any were made
            if (hasChanges)
            {
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Refreshes the reservation list and updates statuses.
        /// Call this when returning to the reservation page.
        /// </summary>
        [RelayCommand]
        public void RefreshReservations()
        {
            LoadData();
        }

        /// <summary>
        /// Opens a dialog to edit the selected reservation.
        /// </summary>
        [RelayCommand]
        public void EditReservation()
        {
            if (SelectedReservation == null)
            {
                MessageBox.Show("Veuillez sélectionner une réservation à modifier.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create a copy of the reservation for editing
            var reservationToEdit = _context.Reservations
                .Include(r => r.Salle)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == SelectedReservation.Id);

            if (reservationToEdit == null)
            {
                MessageBox.Show("La réservation n'a pas pu être chargée.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show edit window
            var editWindow = new ReservationEditWindow(reservationToEdit);
            bool? result = editWindow.ShowDialog();

            if (result == true && editWindow.IsModified)
            {
                try
                {
                    // Validate and save changes
                    _context.Reservations.Update(reservationToEdit);
                    _context.SaveChanges();

                    // Refresh the list
                    LoadData();

                    MessageBox.Show("La réservation a été modifiée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la modification: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
