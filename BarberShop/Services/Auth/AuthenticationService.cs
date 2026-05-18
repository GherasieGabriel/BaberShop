using BarberShop.Models;
using Microsoft.AspNetCore.Identity;

namespace BarberShop.Services.Auth;

public class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IAuthenticationService
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
            EmailConfirmed = true
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

        return (true, "Registration successful! Please log in.");
    }

    public async Task<(bool Success, string Message)> LoginAsync(LoginViewModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return (false, "Invalid email or password");
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
