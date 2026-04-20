namespace BarberShop.Services.Interfaces;

public interface IAdminAccessService
{
    bool IsAdmin();
    bool TryLogin(string username, string password);
    void Logout();
}
