namespace ReserV7.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";

        // Additional fields for room reservation system
        public string Email { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    }
}
