using BarberShop.Models;

namespace BarberShop.Services.Auth;

public interface IAuthenticationService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
    Task<(bool Success, string Message)> LoginAsync(LoginViewModel model);
    Task LogoutAsync();
    Task<ApplicationUser?> GetCurrentUserAsync();
}
