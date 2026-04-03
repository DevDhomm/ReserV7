using Microsoft.EntityFrameworkCore;
using Spacium.Data;
using Spacium.Models;
using Spacium.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace Spacium.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject, INavigationAware
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private bool _isInitialized;

        private int todayReservationsCount;
        private int occupiedRoomsCount;
        private int unavailableRoomsCount;
        private ObservableCollection<UpcomingReservationItem> upcomingReservations = new();
        private ObservableCollection<UpcomingReservationItem> ongoingReservations = new();

        public int TodayReservationsCount
        {
            get => todayReservationsCount;
            private set => SetProperty(ref todayReservationsCount, value);
        }

        public int OccupiedRoomsCount
        {
            get => occupiedRoomsCount;
            private set => SetProperty(ref occupiedRoomsCount, value);
        }

        public int UnavailableRoomsCount
        {
            get => unavailableRoomsCount;
            private set => SetProperty(ref unavailableRoomsCount, value);
        }

        public ObservableCollection<UpcomingReservationItem> UpcomingReservations
        {
            get => upcomingReservations;
            private set => SetProperty(ref upcomingReservations, value);
        }

        public ObservableCollection<UpcomingReservationItem> OngoingReservations
        {
            get => ongoingReservations;
            private set => SetProperty(ref ongoingReservations, value);
        }

        public IRelayCommand GoToBookRoomCommand { get; }
        public IRelayCommand GoToMyReservationsCommand { get; }

        public DashboardViewModel(ApplicationDbContext context, IAuthService authService, INavigationService navigationService)
        {
            _context = context;
            _authService = authService;
            _navigationService = navigationService;
            GoToBookRoomCommand = new RelayCommand(GoToBookRoom);
            GoToMyReservationsCommand = new RelayCommand(GoToMyReservations);
            LoadDashboardData();
        }

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
            }

            LoadDashboardData();
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void GoToBookRoom()
        {
            if (!_authService.IsAuthenticated)
            {
                _navigationService.Navigate(typeof(Views.Pages.LoginPage));
                return;
            }

            _navigationService.Navigate(typeof(Views.Pages.BookRoomPage));
        }

        private void GoToMyReservations()
        {
            if (!_authService.IsAuthenticated)
            {
                _navigationService.Navigate(typeof(Views.Pages.LoginPage));
                return;
            }

            _navigationService.Navigate(typeof(Views.Pages.ReservationPage));
        }

        private void LoadDashboardData()
        {
            var now = DateTime.Now;
            var today = now.Date;

            var allReservations = _context.Reservations
                .Include(r => r.Salle)
                .Include(r => r.User)
                .Where(r => r.Statut != "Annulée")
                .ToList();

            var reservations = allReservations;

            if (_authService.CurrentRole == "User")
            {
                var currentUserName = _authService.CurrentUser ?? string.Empty;
                var user = _context.Users.FirstOrDefault(u => u.Username == currentUserName);
                if (user != null)
                {
                    reservations = allReservations.Where(r => r.UserId == user.Id).ToList();
                }
                else
                {
                    reservations = new List<Reservation>();
                }
            }

            TodayReservationsCount = allReservations.Count(r =>
                TryParseReservationDateTime(r, out var startDateTime, out var endDateTime)
                && today >= startDateTime.Date
                && today <= endDateTime.Date);

            OccupiedRoomsCount = allReservations
                .Where(r => TryParseReservationDateTime(r, out var startDateTime, out var endDateTime)
                            && now >= startDateTime
                            && now <= endDateTime)
                .Select(r => r.SalleId)
                .Distinct()
                .Count();

            UnavailableRoomsCount = _context.Salles.Count(s => !s.Disponibilite);

            var upcomingItems = allReservations
                .Where(r => TryParseReservationDateTime(r, out var startDateTime, out _)
                            && startDateTime >= now)
                .OrderBy(r => DateTime.ParseExact($"{r.DateDebut} {r.HeureDebut}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))
                .Take(5)
                .Select(r => new UpcomingReservationItem
                {
                    SalleNom = r.Salle?.Nom ?? "Salle inconnue",
                    Motif = r.Motif,
                    DateDebut = r.DateDebut,
                    HeureDebut = r.HeureDebut,
                    HeureFin = r.HeureFin,
                    Utilisateur = r.User?.Nom ?? "-"
                })
                .ToList();

            var ongoingItems = allReservations
                .Where(r => TryParseReservationDateTime(r, out var startDateTime, out var endDateTime)
                            && now >= startDateTime
                            && now <= endDateTime)
                .OrderBy(r => DateTime.ParseExact($"{r.DateFin} {r.HeureFin}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))
                .Take(5)
                .Select(r => new UpcomingReservationItem
                {
                    SalleNom = r.Salle?.Nom ?? "Salle inconnue",
                    Motif = r.Motif,
                    DateDebut = r.DateDebut,
                    HeureDebut = r.HeureDebut,
                    HeureFin = r.HeureFin,
                    Utilisateur = r.User?.Nom ?? "-"
                })
                .ToList();

            UpcomingReservations = new ObservableCollection<UpcomingReservationItem>(upcomingItems);
            OngoingReservations = new ObservableCollection<UpcomingReservationItem>(ongoingItems);
        }

        private static bool TryParseReservationDateTime(Reservation reservation, out DateTime startDateTime, out DateTime endDateTime)
        {
            startDateTime = default;
            endDateTime = default;

            if (!DateTime.TryParseExact(reservation.DateDebut, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                return false;

            if (!DateTime.TryParseExact(reservation.DateFin, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                return false;

            if (!TimeSpan.TryParse(reservation.HeureDebut, out var startTime))
                return false;

            if (!TimeSpan.TryParse(reservation.HeureFin, out var endTime))
                return false;

            startDateTime = startDate.Date.Add(startTime);
            endDateTime = endDate.Date.Add(endTime);

            return true;
        }
    }

    public class UpcomingReservationItem
    {
        public string SalleNom { get; set; } = string.Empty;
        public string Motif { get; set; } = string.Empty;
        public string DateDebut { get; set; } = string.Empty;
        public string HeureDebut { get; set; } = string.Empty;
        public string HeureFin { get; set; } = string.Empty;
        public string Utilisateur { get; set; } = string.Empty;
    }
}

