namespace BarberShop.Models;

public class Service
{
    public int ServiceId { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(500)]
    public string? Description { get; set; }

    [System.ComponentModel.DataAnnotations.Range(1, 600)]
    public int BaseDuration { get; set; }

    [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "999999")]
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}