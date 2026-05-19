namespace BarberShop.Models;

public class ProfileViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? MemberSince { get; set; }
    public byte[]? ProfileImage { get; set; }

    public List<Appointment> UpcomingAppointments { get; set; } = new();
    public List<Appointment> PastAppointments { get; set; } = new();
    public Appointment? SelectedAppointment { get; set; }

    public List<string> AvailableServices { get; set; } = new();
    public List<string> AvailableBarbers { get; set; } = new();
}
