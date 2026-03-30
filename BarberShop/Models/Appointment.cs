namespace BarberShop.Models;

public class Appointment
{
    public int AppointmentId { get; set; }
    public int ClientId { get; set; }
    public int BarberId { get; set; }
    public int ServiceId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Client Client { get; set; } = null!;
    public Barber Barber { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}