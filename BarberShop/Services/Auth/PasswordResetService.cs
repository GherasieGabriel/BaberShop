using BarberShop.Models;
using BarberShop.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BarberShop.Services.Auth;

/// <summary>
/// Service for handling password reset and related email communications
/// </summary>
public class PasswordResetService(
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    ILogger<PasswordResetService> logger) : IPasswordResetService
{
    public async Task<(bool Success, string Message)> RequestPasswordResetAsync(string email)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal whether email exists for security reasons
                logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                return (true, "If an account exists with this email, a password reset link has been sent.");
            }

            if (!user.EmailConfirmed)
            {
                logger.LogWarning("Password reset requested for unconfirmed email: {Email}", email);
                return (true, "Please confirm your email address first. Check your inbox for the confirmation link.");
            }

            // Generate password reset token
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            // Build reset link
            var resetLink = $"https://localhost:7293/Account/ResetPassword?userId={user.Id}&token={Uri.EscapeDataString(resetToken)}";

            // Send password reset email
            var emailResult = await emailService.SendPasswordResetEmailAsync(email, resetLink, user.FirstName);

            if (!emailResult.Success)
            {
                logger.LogError("Failed to send password reset email to {Email}: {Message}", email, emailResult.Message);
                return (true, "If an account exists with this email, a password reset link has been sent.");
            }

            logger.LogInformation("Password reset email sent to {Email}", email);
            return (true, "If an account exists with this email, a password reset link has been sent.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error requesting password reset for {Email}", email);
            return (true, "If an account exists with this email, a password reset link has been sent.");
        }
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string userId, string token, string newPassword, string confirmPassword)
    {
        try
        {
            if (newPassword != confirmPassword)
            {
                return (false, "Passwords do not match");
            }

            if (newPassword.Length < 6)
            {
                return (false, "Password must be at least 6 characters long");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            var result = await userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Password reset failed for user {UserId}: {Errors}", userId, errors);
                return (false, $"Password reset failed: {errors}. The link may have expired.");
            }

            logger.LogInformation("Password reset successful for user {UserId}", userId);
            return (true, "Password has been reset successfully. You can now log in with your new password.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during password reset for user {UserId}", userId);
            return (false, "An error occurred while resetting your password. Please try again.");
        }
    }
}

public interface IPasswordResetService
{
    /// <summary>
    /// Requests a password reset by sending an email to the specified address
    /// </summary>
    Task<(bool Success, string Message)> RequestPasswordResetAsync(string email);

    /// <summary>
    /// Resets a user's password using the provided token
    /// </summary>
    Task<(bool Success, string Message)> ResetPasswordAsync(string userId, string token, string newPassword, string confirmPassword);
}
