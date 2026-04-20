using BarberShop.Services.Interfaces;

namespace BarberShop.Services;

public class AdminAccessService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : IAdminAccessService
{
    private const string SessionKey = "IsAdmin";

    public bool IsAdmin()
    {
        var flag = httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
        return string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase);
    }

    public bool TryLogin(string username, string password)
    {
        var configuredUsername = configuration["AdminAccess:Username"] ?? "admin";
        var configuredPassword = configuration["AdminAccess:Password"] ?? "admin123";

        var valid = string.Equals(username?.Trim(), configuredUsername, StringComparison.Ordinal)
                    && string.Equals(password, configuredPassword, StringComparison.Ordinal);

        if (!valid)
        {
            return false;
        }

        httpContextAccessor.HttpContext?.Session.SetString(SessionKey, "true");
        return true;
    }

    public void Logout()
    {
        httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
    }
}
