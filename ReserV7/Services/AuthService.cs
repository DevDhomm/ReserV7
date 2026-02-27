using ReserV7.Data;

namespace ReserV7.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private string? _currentUser;
        private string? _currentRole;

        public string? CurrentUser => _currentUser;

        public string? CurrentRole => _currentRole;

        public bool IsAuthenticated => _currentUser is not null;

        public event Action? UserChanged;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            // Reauete pour recuprer les users de la base de donnes et comparer les crdentials
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
                return false;

            _currentUser = user.Username;
            _currentRole = user.Role;
            UserChanged?.Invoke();

            return true;
        }

        public void Logout()
        {
            _currentUser = null;
            _currentRole = null;
            UserChanged?.Invoke();
        }
    }
}