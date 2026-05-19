using BarberShop.Models;

namespace BarberShop.Services.Profile;

public interface IProfileService
{
    Task<ApplicationUser?> GetProfileAsync(string userId);
    Task<(bool Success, string Message)> UpdateProfileAsync(string userId, UpdateProfileViewModel model);
    Task<(bool Success, string Message)> SendContactMessageAsync(string userId, ContactMessageViewModel model);
    Task<List<ContactMessage>> GetContactMessagesAsync(string userId);
    Task<List<ContactMessage>> GetAllContactMessagesAsync();
    Task<bool> MarkMessageAsReadAsync(int messageId);
}
