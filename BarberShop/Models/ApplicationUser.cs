using Microsoft.AspNetCore.Identity;

namespace BarberShop.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public byte[]? ProfileImage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? BarberId { get; set; }

    // Membership persisted in database (nullable if none)
    public string? MembershipTier { get; set; }
    public DateTime? MembershipExpires { get; set; }
}
