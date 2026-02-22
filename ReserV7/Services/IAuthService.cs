namespace ReserV7.Services
{
    public interface IAuthService
    {
        string? CurrentUser { get; }
        string? CurrentRole { get; }
        bool IsAuthenticated { get; }
        event Action? UserChanged;
        bool Login(string username, string password);
        void Logout();
    }
}