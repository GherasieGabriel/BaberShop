namespace BarberShop.Models;

public class Review
{
    public int ReviewId { get; set; }
    public int AppointmentId { get; set; }
    public int ClientId { get; set; }
    public int BarberId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public Client Client { get; set; } = null!;
    public Barber Barber { get; set; } = null!;
}