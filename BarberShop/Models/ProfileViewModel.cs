namespace BarberShop.Models;

public class ProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? MemberSince { get; set; }

    public List<Appointment> UpcomingAppointments { get; set; } = new();
    public List<Appointment> PastAppointments { get; set; } = new();
    public Appointment? SelectedAppointment { get; set; }
}
