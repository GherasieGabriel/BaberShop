namespace BarberShop.Models;

public class Client
{
    public int ClientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }

    public ClientNotificationSettings? NotificationSettings { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}