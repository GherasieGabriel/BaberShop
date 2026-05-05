namespace BarberShop.Models;

public class Barber
{
    public int BarberId { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.EmailAddress]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string Email { get; set; } = string.Empty;

    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}