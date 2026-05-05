namespace BarberShop.Models;

public class Client
{
    public int ClientId { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    public DateTime MemberSince { get; set; }

    public ClientNotificationSettings? NotificationSettings { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}