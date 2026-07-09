namespace BarberShop.Models;

public record MembershipTier(string Name, int DiscountPercent, string Description);

public class MembershipViewModel
{
    public List<MembershipTier> Tiers { get; set; } = new();
}
