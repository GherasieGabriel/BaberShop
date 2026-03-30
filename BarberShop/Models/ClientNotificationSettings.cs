namespace BarberShop.Models;

public class ClientNotificationSettings
{
    public int ClientId { get; set; }
    public bool NotifyEmail { get; set; }
    public bool NotifySms { get; set; }

    public Client Client { get; set; } = null!;
}