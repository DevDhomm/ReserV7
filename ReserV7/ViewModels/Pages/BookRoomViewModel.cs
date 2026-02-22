using Microsoft.EntityFrameworkCore;
using ReserV7.Data;
using ReserV7.Models;
using ReserV7.Services;
using ReserV7.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows;

namespace ReserV7.ViewModels.Pages
{
    public partial class BookRoomViewModel : ObservableObject
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private List<Salle> _allRooms = new();

        [ObservableProperty]
        private ObservableCollection<Salle> salles = new();

        [ObservableProperty]
        private ObservableCollection<Equipement> availableEquipements = new();

        [ObservableProperty]
        private Salle? selectedSalle;

        [ObservableProperty]
        private string selectedMotif = string.Empty;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Now;

        [ObservableProperty]
        private DateTime selectedDateStart = DateTime.Now;

        [ObservableProperty]
        private DateTime selectedDateEnd = DateTime.Now;

        [ObservableProperty]
        private DateTime selectedEndDate = DateTime.Now;

        [ObservableProperty]
        private string selectedTimeStart = "09:00";

        [ObservableProperty]
        private string selectedTimeEnd = "10:00";

        [ObservableProperty]
        private bool isBookingPanelOpen = false;

        [ObservableProperty]
        private bool useCustomTimes = false;

        [ObservableProperty]
        private ObservableCollection<CreneauDisplay> availableCreneaux = new();

        [ObservableProperty]
        private CreneauDisplay? selectedCreneau;

        [ObservableProperty]
        private string? availabilityMessage;

        // Filter properties
        [ObservableProperty]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        private int minCapacity = 0;

        [ObservableProperty]
        private int selectedFloor = 0;

        [ObservableProperty]
        private ObservableCollection<EquipementFilter> equipementFilters = new();

        public BookRoomViewModel(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
            LoadData();
        }

        private void LoadData()
        {
            _allRooms = _context.Salles
                .Include(s => s.Equipements)
                .Where(s => s.Disponibilite)
                .ToList();

            // Load available equipments for filter
            var equipements = _context.Equipements.Distinct().ToList();
            EquipementFilters = new ObservableCollection<EquipementFilter>(
                equipements.Select(e => new EquipementFilter { Id = e.Id, Nom = e.Nom, IsSelected = false })
            );

            ApplyFilters();
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            var filtered = _allRooms.AsEnumerable();

            // Filtre par nom
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                filtered = filtered.Where(s => s.Nom.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            // Filtre par capacité minimale
            if (MinCapacity > 0)
            {
                filtered = filtered.Where(s => s.Capacite >= MinCapacity);
            }

            // Filtre par étage
            if (SelectedFloor > 0)
            {
                filtered = filtered.Where(s => s.Etage == SelectedFloor);
            }

            // Filtre par équipements
            var selectedEquipementIds = EquipementFilters
                .Where(e => e.IsSelected)
                .Select(e => e.Id)
                .ToList();

            if (selectedEquipementIds.Any())
            {
                filtered = filtered.Where(s =>
                    s.Equipements.Any(e => selectedEquipementIds.Contains(e.Id))
                );
            }

            Salles = new ObservableCollection<Salle>(filtered.ToList());
        }

        [RelayCommand]
        private void ResetFilters()
        {
            SearchQuery = string.Empty;
            MinCapacity = 0;
            SelectedFloor = 0;

            foreach (var eq in EquipementFilters)
            {
                eq.IsSelected = false;
            }

            ApplyFilters();
        }

        [RelayCommand]
        private void SelectRoomForBooking(Salle? room)
        {
            if (room != null)
            {
                SelectedSalle = room;
                SelectedDate = DateTime.Now;
                SelectedEndDate = DateTime.Now;
                LoadAvailableCreneaux();
                UseCustomTimes = false;
                SelectedCreneau = null;
                AvailabilityMessage = null;

                // Open reservation window
                var reservationWindow = new ReservationWindow(this)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                reservationWindow.ShowDialog();
            }
        }

        private void LoadAvailableCreneaux()
        {
            if (SelectedSalle == null)
                return;

            // Get all creneaux
            var creneaux = _context.Creneaux.ToList();

            // Get existing reservations for this room on the selected date
            var selectedDateStr = SelectedDate.ToString("yyyy-MM-dd");
            var existingReservations = _context.Reservations
                .Where(r => r.SalleId == SelectedSalle.Id && r.DateDebut == selectedDateStr)
                .ToList();

            var availableCreneaux = new List<CreneauDisplay>();

            foreach (var creneau in creneaux)
            {
                // Check if this creneau conflicts with any existing reservation
                bool hasConflict = existingReservations.Any(r =>
                {
                    if (!TimeSpan.TryParse(r.HeureDebut, out var reservationStart) ||
                        !TimeSpan.TryParse(r.HeureFin, out var reservationEnd))
                        return false;

                    return !(creneau.Fin <= reservationStart || creneau.Debut >= reservationEnd);
                });

                availableCreneaux.Add(new CreneauDisplay
                {
                    CreneauId = creneau.Id,
                    Debut = creneau.Debut,
                    Fin = creneau.Fin,
                    IsAvailable = !hasConflict,
                    DisplayText = $"{creneau.Debut:hh\\:mm} - {creneau.Fin:hh\\:mm}"
                });
            }

            AvailableCreneaux = new ObservableCollection<CreneauDisplay>(
                availableCreneaux.OrderBy(c => c.Debut)
            );
        }

        partial void OnSelectedDateStartChanged(DateTime value)
        {
            // Sync with SelectedDate
            SelectedDate = value;
        }

        partial void OnSelectedDateEndChanged(DateTime value)
        {
            // Sync with SelectedEndDate
            SelectedEndDate = value;
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            // Reload available creneaux when date changes
            LoadAvailableCreneaux();
        }

        [RelayCommand]
        private void ConfirmBooking()
        {
            // Validation 1: Champs obligatoires
            if (SelectedSalle == null)
            {
                ShowError("❌ Erreur", "Aucune salle n'est sélectionnée");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedMotif))
            {
                ShowError("❌ Motif manquant", "Veuillez entrer un motif de réservation");
                return;
            }

            // Validation 2: Utilisateur
            var currentUserName = _authService.CurrentUser ?? "";
            var user = _context.Users.FirstOrDefault(u => u.Username == currentUserName);

            if (user == null)
            {
                ShowError("❌ Erreur utilisateur", "Impossible de trouvercet utilisateur");
                return;
            }

            // Validation 3: Dates
            if (SelectedDate > SelectedEndDate)
            {
                ShowError("❌ Dates invalides", "La date de fin doit être après la date de début");
                return;
            }

            TimeSpan startTime, endTime;

            // Validation 4: Horaires
            if (UseCustomTimes)
            {
                if (!TimeSpan.TryParse(SelectedTimeStart, out startTime) || 
                    !TimeSpan.TryParse(SelectedTimeEnd, out endTime))
                {
                    ShowError("❌ Format d'heure invalide", "Utilisez le format HH:mm (ex: 09:00)");
                    return;
                }

                if (startTime >= endTime)
                {
                    ShowError("❌ Horaires invalides", "L'heure de fin doit être après l'heure de début");
                    return;
                }
            }
            else
            {
                if (SelectedCreneau == null)
                {
                    ShowError("❌ Créneau manquant", "Veuillez sélectionner un créneau horaire");
                    return;
                }

                if (!SelectedCreneau.IsAvailable)
                {
                    ShowError("❌ Créneau indisponible", "Ce créneau n'est plus disponible");
                    return;
                }

                startTime = SelectedCreneau.Debut;
                endTime = SelectedCreneau.Fin;
            }

            // Validation 5: Vérifier les conflits pour toutes les dates entre dateDebut et dateFin
            var dateStart = SelectedDate.ToString("yyyy-MM-dd");
            var dateEnd = SelectedEndDate.ToString("yyyy-MM-dd");
            var startTimeStr = startTime.ToString(@"hh\:mm");
            var endTimeStr = endTime.ToString(@"hh\:mm");

            // Générer toutes les dates entre dateStart et dateEnd
            var conflictDates = new List<DateTime>();
            for (var date = SelectedDate; date <= SelectedEndDate; date = date.AddDays(1))
            {
                var dateStr = date.ToString("yyyy-MM-dd");

                // Vérifier les conflits pour cette date
                var dayConflicts = _context.Reservations
                    .Where(r => r.SalleId == SelectedSalle.Id && r.DateDebut == dateStr)
                    .ToList()
                    .Where(r =>
                    {
                        if (!TimeSpan.TryParse(r.HeureDebut, out var reservationStart) ||
                            !TimeSpan.TryParse(r.HeureFin, out var reservationEnd))
                            return false;

                        return !(endTime <= reservationStart || startTime >= reservationEnd);
                    })
                    .Any();

                if (dayConflicts)
                {
                    conflictDates.Add(date);
                }
            }

            // Si conflits trouvés
            if (conflictDates.Any())
            {
                var conflictMsg = $"Conflit(s) détecté(s) le(s) :\n{string.Join(", ", conflictDates.Select(d => d.ToString("dd/MM/yyyy")))}";

                var dialog = new DialogWindow(
                    "⚠️ Conflits de Réservation",
                    $"Cette salle a des réservations en conflit:\n\n{conflictMsg}\n\nVoulez-vous vraiment continuer?",
                    DialogWindow.DialogType.Warning,
                    showSecondaryButton: true
                )
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                dialog.ShowDialog();

                if (dialog.DialogResult != DialogWindow.CustomDialogResult.Yes)
                {
                    AvailabilityMessage = "⚠️ Réservation annulée";
                    return;
                }
            }

            // Créer la réservation multi-jours
            try
            {
                var reservation = new Reservation
                {
                    SalleId = SelectedSalle.Id,
                    Motif = SelectedMotif,
                    DateDebut = dateStart,
                    DateFin = dateEnd,
                    HeureDebut = startTimeStr,
                    HeureFin = endTimeStr,
                    Statut = "Confirmée",
                    UserId = user.Id
                };

                _context.Reservations.Add(reservation);
                _context.SaveChanges();

                // Afficher le message de succès
                var daysCount = (SelectedEndDate - SelectedDate).Days + 1;
                var dialog = new DialogWindow(
                    "Réservation Confirmée",
                    $"✅ Réservation confirmée!\n\n" +
                    $"Salle: {SelectedSalle.Nom}\n" +
                    $"Du {SelectedDate:dd/MM/yyyy} au {SelectedEndDate:dd/MM/yyyy} ({daysCount} jour(s))\n" +
                    $"Horaire: {startTimeStr} - {endTimeStr}",
                    DialogWindow.DialogType.Information
                )
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                dialog.ShowDialog();

                // Réinitialiser le formulaire
                ResetForm();

                // Fermer la fenêtre
                Application.Current.Windows.OfType<ReservationWindow>().FirstOrDefault()?.Close();
            }
            catch (Exception ex)
            {
                ShowError("❌ Erreur système", $"Impossible de créer la réservation:\n{ex.Message}");
            }
        }

        private void ShowError(string title, string message)
        {
            var dialog = new DialogWindow(title, message, DialogWindow.DialogType.Error)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.ShowDialog();
            AvailabilityMessage = message;
        }

        private void ResetForm()
        {
            SelectedMotif = string.Empty;
            SelectedDate = DateTime.Now;
            SelectedEndDate = DateTime.Now;
            SelectedTimeStart = "09:00";
            SelectedTimeEnd = "10:00";
            SelectedSalle = null;
            UseCustomTimes = false;
            SelectedCreneau = null;
            AvailabilityMessage = null;

            // Refresh the list of rooms
            LoadData();
        }

        [RelayCommand]
        private void RefreshRooms()
        {
            LoadData();
        }
    }

    public partial class CreneauDisplay : ObservableObject
    {
        public int CreneauId { get; set; }
        public TimeSpan Debut { get; set; }
        public TimeSpan Fin { get; set; }
        public bool IsAvailable { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    public partial class EquipementFilter : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string nom = string.Empty;

        [ObservableProperty]
        private bool isSelected;
    }
}
