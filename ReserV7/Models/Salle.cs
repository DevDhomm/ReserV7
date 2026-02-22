namespace ReserV7.Models
{
    public class Salle
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Capacite { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Etage { get; set; }
        public bool Disponibilite { get; set; } = true;
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Equipement> Equipements { get; set; } = new List<Equipement>();
    }
}
