using BarberShop.Models;
using BarberShop.Services.Interfaces;
using BarberShop.Services.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public class AdminController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IAdminAccessService adminAccessService,
    IProfileService profileService) : Controller
{
    [HttpGet("")]
    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpGet("Users")]
    public async Task<IActionResult> Users()
    {
        var users = await userManager.Users.ToListAsync();
        return View(users);
    }

    [HttpGet("UserRoles/{userId}")]
    public async Task<IActionResult> UserRoles(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await userManager.GetRolesAsync(user);
        var allRoles = roleManager.Roles.Select(r => r.Name).ToList();

        var model = new UserRolesViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CurrentRoles = userRoles.ToList(),
            AvailableRoles = allRoles!
        };

        return View(model);
    }

    [HttpPost("AssignRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var roleExists = await roleManager.RoleExistsAsync(role);
        if (!roleExists)
        {
            TempData["ErrorMessage"] = "Role does not exist";
            return RedirectToAction("UserRoles", new { userId });
        }

        var isInRole = await userManager.IsInRoleAsync(user, role);
        if (!isInRole)
        {
            await userManager.AddToRoleAsync(user, role);
            TempData["SuccessMessage"] = $"Role '{role}' assigned successfully";
        }

        return RedirectToAction("UserRoles", new { userId });
    }

    [HttpPost("RemoveRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(string userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var isInRole = await userManager.IsInRoleAsync(user, role);
        if (isInRole)
        {
            await userManager.RemoveFromRoleAsync(user, role);
            TempData["SuccessMessage"] = $"Role '{role}' removed successfully";
        }

        return RedirectToAction("UserRoles", new { userId });
    }

    [HttpPost("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Invalid user id.";
            return RedirectToAction("Users");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Users");
        }

        // Prevent admin from deleting themselves
        var currentUserId = userManager.GetUserId(User);
        if (string.Equals(currentUserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToAction("Users");
        }

        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "User deleted successfully.";
        }
        else
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            TempData["ErrorMessage"] = $"Failed to delete user: {errors}";
        }

        return RedirectToAction("Users");
    }

    [HttpGet("Messages")]
    public async Task<IActionResult> Messages(int? selectedMessageId = null, bool unreadOnly = false)
    {
        var allMessages = await profileService.GetAllContactMessagesAsync();
        var filteredMessages = unreadOnly
            ? allMessages.Where(m => !m.IsRead).ToList()
            : allMessages;

        var model = new AdminInboxViewModel
        {
            Messages = await BuildInboxItemsAsync(filteredMessages),
            TotalCount = allMessages.Count,
            UnreadCount = allMessages.Count(m => !m.IsRead),
            UnreadOnly = unreadOnly,
            SelectedMessageId = selectedMessageId
        };

        if (selectedMessageId.HasValue)
        {
            var selected = await profileService.GetContactMessageByIdAsync(selectedMessageId.Value);
            if (selected != null)
            {
                model.SelectedMessage = await BuildInboxItemAsync(selected);
            }
        }
        else if (model.Messages.Any())
        {
            model.SelectedMessage = model.Messages.First();
            model.SelectedMessageId = model.SelectedMessage.MessageId;
        }

        return View(model);
    }

    [HttpPost("Messages/MarkRead")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkMessageRead(int messageId, bool unreadOnly = false)
    {
        var success = await profileService.MarkMessageAsReadAsync(messageId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Message marked as read." : "Message not found.";
        return RedirectToAction(nameof(Messages), new { selectedMessageId = messageId, unreadOnly });
    }

    [HttpPost("Messages/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(int messageId, bool unreadOnly = false)
    {
        var success = await profileService.DeleteMessageAsync(messageId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Message deleted." : "Message not found.";
        return RedirectToAction(nameof(Messages), new { unreadOnly });
    }

    private async Task<List<AdminInboxMessageItemViewModel>> BuildInboxItemsAsync(IEnumerable<ContactMessage> messages)
    {
        var items = new List<AdminInboxMessageItemViewModel>();
        foreach (var message in messages)
        {
            items.Add(await BuildInboxItemAsync(message));
        }

        return items.OrderByDescending(m => m.CreatedAt).ToList();
    }

    private async Task<AdminInboxMessageItemViewModel> BuildInboxItemAsync(ContactMessage message)
    {
        var user = await userManager.FindByIdAsync(message.UserId);
        var displayName = user is null
            ? "Unknown sender"
            : $"{user.FirstName} {user.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = user?.UserName ?? "Unknown sender";
        }

        return new AdminInboxMessageItemViewModel
        {
            MessageId = message.MessageId,
            UserId = message.UserId,
            SenderName = displayName,
            SenderEmail = user?.Email ?? string.Empty,
            Subject = message.Subject,
            Message = message.Message,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }
}

