namespace BarberShop.Models;

public class Service
{
    public int ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BaseDuration { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}