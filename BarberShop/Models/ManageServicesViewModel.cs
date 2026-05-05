namespace BarberShop.Models;

public class ManageServicesViewModel
{
    public List<Service> Services { get; set; } = new();
    public Service NewService { get; set; } = new();
}
