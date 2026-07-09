namespace BarberShop.Models;

public class SpecialOfferViewModel
{
    public Service? Service { get; set; }
    public int DiscountPercent { get; set; }
    public decimal DiscountedPrice { get; set; }
}
