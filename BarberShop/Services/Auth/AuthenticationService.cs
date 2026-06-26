using BarberShop.Models;
using BarberShop.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BarberShop.Services.Auth;

public class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailService emailService,
    ILogger<AuthenticationService> logger) : IAuthenticationService
{
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
    {
        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return (false, "User with this email already exists");
        }

        // Create new user
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = false  // Changed to false - will be confirmed via email
        };

        // Create user with password
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, $"Registration failed: {errors}");
        }

        // Assign default User role
        await userManager.AddToRoleAsync(user, "User");

        // Generate email confirmation token
        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

        // Build confirmation link (note: you'll need to inject IHttpContextAccessor or pass URL differently)
        var confirmationLink = $"https://localhost:7293/Account/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(confirmationToken)}";

        // Send welcome email with confirmation link
        try
        {
            var emailResult = await emailService.SendWelcomeEmailAsync(
                user.Email,
                user.FirstName,
                confirmationLink);

            if (!emailResult.Success)
            {
                logger.LogWarning("Failed to send welcome email to {Email}: {Message}", user.Email, emailResult.Message);
                // Don't fail registration if email fails, just log it
            }
            else
            {
                logger.LogInformation("Welcome email sent successfully to {Email}", user.Email);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while sending welcome email to {Email}", user.Email);
            // Don't fail registration if email fails
        }

        return (true, "Registration successful! Please check your email to verify your account.");
    }

    public async Task<(bool Success, string Message)> LoginAsync(LoginViewModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return (false, "Invalid email or password");
        }

        // Check if email is confirmed
        if (!user.EmailConfirmed)
        {
            return (false, "Please confirm your email address before logging in. Check your email for the confirmation link.");
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return (false, "Invalid email or password");
        }

        return (true, "Login successful");
    }

    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync();
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var user = await signInManager.UserManager.GetUserAsync(null);
        return user;
    }
}
