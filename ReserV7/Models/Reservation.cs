namespace ReserV7.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string DateReservation { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        public string Motif { get; set; } = string.Empty;
        public string Statut { get; set; } = "Confirmée"; // En attente, Confirmée, Annulée, Terminée

        public int UserId { get; set; }
        public int SalleId { get; set; }
        public int? CreneauId { get; set; }

        public string DateDebut { get; set; } = string.Empty; // Format: YYYY-MM-DD
        public string DateFin { get; set; } = string.Empty;   // Format: YYYY-MM-DD
        public string HeureDebut { get; set; } = string.Empty; // Format: HH:mm
        public string HeureFin { get; set; } = string.Empty;   // Format: HH:mm

        // Navigation properties
        public User? User { get; set; }
        public Salle? Salle { get; set; }
        public Creneau? Creneau { get; set; }
    }
}
