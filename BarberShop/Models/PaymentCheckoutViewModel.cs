namespace BarberShop.Models;

public class PaymentCheckoutViewModel
{
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RON";
}
