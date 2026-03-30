namespace BarberShop.Models;

public class Barber
{
    public int BarberId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}