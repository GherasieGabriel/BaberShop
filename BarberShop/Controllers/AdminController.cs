using BarberShop.Models;
using BarberShop.Services.Interfaces;
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
    IAdminAccessService adminAccessService) : Controller
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

    [HttpGet("Messages")]
    public async Task<IActionResult> Messages()
    {
        var messages = await userManager.Users
            .SelectMany(u => u.Id == u.Id ? new[] { u } : Array.Empty<ApplicationUser>())
            .ToListAsync();

        // This will need integration with message service
        return View();
    }
}

