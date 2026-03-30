namespace BarberShop.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public int AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RON";
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string? TransactionRef { get; set; }
    public DateTime CreatedAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
}