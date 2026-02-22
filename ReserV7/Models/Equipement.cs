namespace ReserV7.Models
{
    public class Equipement
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool EstFonctionnel { get; set; } = true;
        public int SalleId { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Salle? Salle { get; set; }
    }
}
