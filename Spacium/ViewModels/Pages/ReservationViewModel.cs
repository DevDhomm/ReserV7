using Microsoft.EntityFrameworkCore;
using Spacium.Data;
using Spacium.Models;
using Spacium.Services;
using Spacium.Views.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace Spacium.ViewModels.Pages
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
        private void DeleteReservation(Reservation reservation)
        {
            if (reservation == null)
            {
                MessageBox.Show("Veuillez sélectionner une réservation à supprimer.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer cette réservation pour {reservation.Salle?.Nom}?",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Reservations.Remove(reservation);
                    _context.SaveChanges();
                    _allReservations.Remove(reservation);
                    ApplyFilters();
                    MessageBox.Show("La réservation a été supprimée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void CancelReservation(Reservation reservation)
        {
            if (reservation == null)
            {
                MessageBox.Show("Veuillez sélectionner une réservation à annuler.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if reservation can be cancelled
            if (reservation.Statut == "Annulée")
            {
                MessageBox.Show("Cette réservation est déjà annulée.", "Déjà annulée", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (reservation.Statut == "Terminée")
            {
                MessageBox.Show("Vous ne pouvez pas annuler une réservation terminée.", "Action non autorisée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir annuler cette réservation pour {reservation.Salle?.Nom}?",
                "Confirmation d'annulation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    reservation.Statut = "Annulée";
                    _context.Reservations.Update(reservation);
                    _context.SaveChanges();

                    // Refresh the list
                    LoadData();

                    MessageBox.Show("La réservation a été annulée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'annulation: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
        /// Only allows editing for reservations with status "En attente" or "Confirmée".
        /// </summary>
        [RelayCommand]
        public void EditReservation(Reservation reservation)
        {
            if (reservation == null)
            {
                MessageBox.Show("Veuillez sélectionner une réservation à modifier.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if reservation can be edited based on status
            if (reservation.Statut == "Terminée")
            {
                MessageBox.Show("Vous ne pouvez pas modifier une réservation terminée.", "Action non autorisée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (reservation.Statut == "En cours")
            {
                MessageBox.Show("Vous ne pouvez pas modifier une réservation en cours.", "Action non autorisée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (reservation.Statut == "Annulée")
            {
                MessageBox.Show("Vous ne pouvez pas modifier une réservation annulée.", "Action non autorisée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reservationEntity = _context.Reservations
                .Include(r => r.Salle)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == reservation.Id);

            if (reservationEntity == null)
            {
                MessageBox.Show("La réservation n'a pas pu être chargée.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var reservationToEdit = CreateEditableCopy(reservationEntity);

            // Show edit window
            var editWindow = new ReservationEditWindow(reservationToEdit);
            bool? result = editWindow.ShowDialog();

            if (result == true && editWindow.IsModified)
            {
                try
                {
                    if (TryGetConflictingReservation(reservationToEdit, out var conflictingReservation))
                    {
                        MessageBox.Show(
                            $"Conflit détecté avec une autre réservation pour la salle {reservationToEdit.Salle?.Nom}.\n" +
                            $"Réservation en conflit: {conflictingReservation?.DateDebut} au {conflictingReservation?.DateFin} " +
                            $"de {conflictingReservation?.HeureDebut} à {conflictingReservation?.HeureFin}.",
                            "Chevauchement détecté",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    ApplyEditedValues(reservationEntity, reservationToEdit);
                    _context.Reservations.Update(reservationEntity);
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

        private static Reservation CreateEditableCopy(Reservation source)
        {
            return new Reservation
            {
                Id = source.Id,
                DateReservation = source.DateReservation,
                Motif = source.Motif,
                Statut = source.Statut,
                UserId = source.UserId,
                SalleId = source.SalleId,
                CreneauId = source.CreneauId,
                DateDebut = source.DateDebut,
                DateFin = source.DateFin,
                HeureDebut = source.HeureDebut,
                HeureFin = source.HeureFin,
                User = source.User,
                Salle = source.Salle,
                Creneau = source.Creneau
            };
        }

        private static void ApplyEditedValues(Reservation target, Reservation source)
        {
            target.Motif = source.Motif;
            target.Statut = source.Statut;
            target.DateDebut = source.DateDebut;
            target.DateFin = source.DateFin;
            target.HeureDebut = source.HeureDebut;
            target.HeureFin = source.HeureFin;
            target.CreneauId = source.CreneauId;
        }

        private bool TryGetConflictingReservation(Reservation reservationToEdit, out Reservation? conflictingReservation)
        {
            conflictingReservation = null;

            if (!TryParseReservationRange(reservationToEdit, out var editStartDate, out var editEndDate, out var editStartTime, out var editEndTime))
            {
                return false;
            }

            var reservationsInSameRoom = _context.Reservations
                .Where(r => r.SalleId == reservationToEdit.SalleId
                            && r.Id != reservationToEdit.Id
                            && r.Statut != "Annulée")
                .ToList();

            foreach (var existingReservation in reservationsInSameRoom)
            {
                if (!TryParseReservationRange(existingReservation, out var existingStartDate, out var existingEndDate, out var existingStartTime, out var existingEndTime))
                {
                    continue;
                }

                var datesOverlap = editStartDate <= existingEndDate && editEndDate >= existingStartDate;
                var timesOverlap = editStartTime < existingEndTime && editEndTime > existingStartTime;

                if (datesOverlap && timesOverlap)
                {
                    conflictingReservation = existingReservation;
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseReservationRange(
            Reservation reservation,
            out DateTime startDate,
            out DateTime endDate,
            out TimeSpan startTime,
            out TimeSpan endTime)
        {
            startDate = default;
            endDate = default;
            startTime = default;
            endTime = default;

            var dateStyle = DateTimeStyles.AssumeLocal;
            var dateCulture = CultureInfo.InvariantCulture;

            var validDates = DateTime.TryParseExact(reservation.DateDebut, "yyyy-MM-dd", dateCulture, dateStyle, out startDate)
                             && DateTime.TryParseExact(reservation.DateFin, "yyyy-MM-dd", dateCulture, dateStyle, out endDate);

            var validTimes = TimeSpan.TryParse(reservation.HeureDebut, out startTime)
                             && TimeSpan.TryParse(reservation.HeureFin, out endTime);

            return validDates && validTimes;
        }
    }
}

