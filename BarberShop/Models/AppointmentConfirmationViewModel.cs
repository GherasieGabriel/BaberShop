namespace BarberShop.Models;

public class AppointmentConfirmationViewModel
{
    public Appointment Appointment { get; set; } = null!;
    public string Message { get; set; } = "Your appointment has been booked successfully.";
    public bool HasPaymentDue { get; set; } = true;
}
