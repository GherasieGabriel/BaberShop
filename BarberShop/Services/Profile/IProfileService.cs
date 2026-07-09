using BarberShop.Models;

namespace BarberShop.Services.Profile;

public interface IProfileService
{
    Task<ApplicationUser?> GetProfileAsync(string userId);
    Task<(bool Success, string Message)> UpdateProfileAsync(string userId, UpdateProfileViewModel model);
    Task<(bool Success, string Message)> SendContactMessageAsync(string userId, ContactMessageViewModel model);
    Task<List<ContactMessage>> GetContactMessagesAsync(string userId);
    Task<List<ContactMessage>> GetAllContactMessagesAsync(bool unreadOnly = false);
    Task<ContactMessage?> GetContactMessageByIdAsync(int messageId);
    Task<bool> MarkMessageAsReadAsync(int messageId);
    Task<bool> DeleteMessageAsync(int messageId);
}
