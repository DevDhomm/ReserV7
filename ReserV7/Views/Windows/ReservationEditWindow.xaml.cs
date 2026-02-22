using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ReserV7.Models;

namespace ReserV7.Views.Windows
{
    public partial class ReservationEditWindow : Window, INotifyPropertyChanged
    {
        public Reservation? SelectedReservation { get; set; }
        public bool IsModified { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ReservationEditWindow(Reservation reservation)
        {
            InitializeComponent();
            SelectedReservation = reservation;
            DataContext = this;
            IsModified = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                IsModified = true;
                DialogResult = true;
                Close();
            }
        }

        private bool ValidateInput()
        {
            if (SelectedReservation == null)
                return false;

            // Validate dates
            if (!DateTime.TryParse(StartDateDisplay, out DateTime startDate) ||
                !DateTime.TryParse(EndDateDisplay, out DateTime endDate))
            {
                MessageBox.Show("Les dates doivent être au format JJ/MM/AAAA.", "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (endDate < startDate)
            {
                MessageBox.Show("La date de fin ne peut pas être avant la date de début.", "Dates invalides", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate times
            if (!TimeOnly.TryParse(SelectedReservation.HeureDebut, out TimeOnly startTime) ||
                !TimeOnly.TryParse(SelectedReservation.HeureFin, out TimeOnly endTime))
            {
                MessageBox.Show("Les horaires doivent être au format HH:mm.", "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (endTime <= startTime)
            {
                MessageBox.Show("L'heure de fin doit être après l'heure de début.", "Horaires invalides", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate motif
            if (string.IsNullOrWhiteSpace(SelectedReservation.Motif))
            {
                MessageBox.Show("Le motif de la réservation ne peut pas être vide.", "Motif requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        public string StartDateDisplay
        {
            get
            {
                if (DateTime.TryParse(SelectedReservation?.DateDebut, out DateTime date))
                {
                    return date.ToString("dd/MM/yyyy");
                }
                return "";
            }
            set
            {
                if (SelectedReservation != null && DateTime.TryParse(value, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    SelectedReservation.DateDebut = date.ToString("yyyy-MM-dd");
                    OnPropertyChanged(nameof(StartDateDisplay));
                }
            }
        }

        public string EndDateDisplay
        {
            get
            {
                if (DateTime.TryParse(SelectedReservation?.DateFin, out DateTime date))
                {
                    return date.ToString("dd/MM/yyyy");
                }
                return "";
            }
            set
            {
                if (SelectedReservation != null && DateTime.TryParse(value, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    SelectedReservation.DateFin = date.ToString("yyyy-MM-dd");
                    OnPropertyChanged(nameof(EndDateDisplay));
                }
            }
        }

        public DateTime? StartDatePickerValue
        {
            get
            {
                if (DateTime.TryParse(SelectedReservation?.DateDebut, out DateTime date))
                {
                    return date;
                }
                return null;
            }
            set
            {
                if (SelectedReservation != null && value.HasValue)
                {
                    SelectedReservation.DateDebut = value.Value.ToString("yyyy-MM-dd");
                    OnPropertyChanged(nameof(StartDatePickerValue));
                }
            }
        }

        public DateTime? EndDatePickerValue
        {
            get
            {
                if (DateTime.TryParse(SelectedReservation?.DateFin, out DateTime date))
                {
                    return date;
                }
                return null;
            }
            set
            {
                if (SelectedReservation != null && value.HasValue)
                {
                    SelectedReservation.DateFin = value.Value.ToString("yyyy-MM-dd");
                    OnPropertyChanged(nameof(EndDatePickerValue));
                }
            }
        }

        public ObservableCollection<string> AvailableStatuts
        {
            get
            {
                return new ObservableCollection<string>
                {
                    "En attente",
                    "Confirmée",
                    "En cours",
                    "Terminée",
                    "Annulée"
                };
            }
        }

        public bool ShowInfoMessage => !string.IsNullOrEmpty(InfoMessage);

        public string InfoMessage
        {
            get
            {
                if (SelectedReservation?.Statut == "Annulée")
                {
                    return "⚠️ Cette réservation est marquée comme annulée. Les modifications seront sauvegardées.";
                }
                return "";
            }
        }
    }
}
