namespace BarberShop.Models;

public class ManageProductsViewModel
{
    public List<Product> Products { get; set; } = new();
    public Product NewProduct { get; set; } = new();
}
