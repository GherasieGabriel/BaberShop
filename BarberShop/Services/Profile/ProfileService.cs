using BarberShop.Data;
using BarberShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Services.Profile;

public class ProfileService(
    UserManager<ApplicationUser> userManager,
    BarberShopDbContext dbContext) : IProfileService
{
    public async Task<ApplicationUser?> GetProfileAsync(string userId)
    {
        return await userManager.FindByIdAsync(userId);
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(string userId, UpdateProfileViewModel model)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Update failed: {errors}");
            }

            return (true, "Profile updated successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating profile: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> SendContactMessageAsync(string userId, ContactMessageViewModel model)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            var message = new ContactMessage
            {
                UserId = userId,
                Subject = model.Subject,
                Message = model.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.ContactMessages.AddAsync(message);
            await dbContext.SaveChangesAsync();

            return (true, "Message sent successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error sending message: {ex.Message}");
        }
    }

    public async Task<List<ContactMessage>> GetContactMessagesAsync(string userId)
    {
        return await dbContext.ContactMessages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ContactMessage>> GetAllContactMessagesAsync(bool unreadOnly = false)
    {
        var query = dbContext.ContactMessages.AsQueryable();
        if (unreadOnly)
        {
            query = query.Where(m => !m.IsRead);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<ContactMessage?> GetContactMessageByIdAsync(int messageId)
    {
        return await dbContext.ContactMessages
            .FirstOrDefaultAsync(m => m.MessageId == messageId);
    }

    public async Task<bool> MarkMessageAsReadAsync(int messageId)
    {
        try
        {
            var message = await dbContext.ContactMessages.FindAsync(messageId);
            if (message == null)
            {
                return false;
            }

            if (message.IsRead)
            {
                return true;
            }

            message.IsRead = true;
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteMessageAsync(int messageId)
    {
        try
        {
            var message = await dbContext.ContactMessages.FindAsync(messageId);
            if (message == null)
            {
                return false;
            }

            dbContext.ContactMessages.Remove(message);
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
