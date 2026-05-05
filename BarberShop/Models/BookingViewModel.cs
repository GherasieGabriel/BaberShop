using System.ComponentModel.DataAnnotations;

namespace BarberShop.Models;

public class BookingViewModel
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Service { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Barber { get; set; } = string.Empty;

    [Required]
    public DateTime? Date { get; set; }

    [Required]
    public TimeSpan? Time { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public List<string> AvailableServices { get; set; } = new();
    public List<string> AvailableBarbers { get; set; } = new();
}
