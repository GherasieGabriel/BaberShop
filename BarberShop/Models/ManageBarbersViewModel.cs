namespace BarberShop.Models;

public class ManageBarbersViewModel
{
    public List<Barber> Barbers { get; set; } = new();
    public Barber NewBarber { get; set; } = new();
}
