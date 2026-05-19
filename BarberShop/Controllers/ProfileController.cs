using BarberShop.Models;
using BarberShop.Services.Image;
using BarberShop.Services.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

[Route("Profile")]
[Authorize]
public class ProfileController(
    UserManager<ApplicationUser> userManager,
    IProfileService profileService,
    IImageService imageService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(user);
    }

    [HttpGet("Edit")]
    public async Task<IActionResult> Edit()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = new UpdateProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        };

        return View(model);
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var (success, message) = await profileService.UpdateProfileAsync(user.Id, model);
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        TempData["SuccessMessage"] = message;
        return RedirectToAction("Index");
    }

    [HttpPost("UploadImage")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(IFormFile? profileImage)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (profileImage == null || profileImage.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select an image to upload";
            return RedirectToAction("Index");
        }

        var (success, message) = await imageService.UploadProfileImageAsync(user.Id, profileImage);
        if (!success)
        {
            TempData["ErrorMessage"] = message;
        }
        else
        {
            TempData["SuccessMessage"] = message;
        }

        return RedirectToAction("Index");
    }

    [HttpPost("DeleteImage")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var success = await imageService.DeleteProfileImageAsync(user.Id);
        if (success)
        {
            TempData["SuccessMessage"] = "Profile image deleted successfully";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete profile image";
        }

        return RedirectToAction("Index");
    }

    [HttpGet("Messages")]
    public async Task<IActionResult> Messages()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var messages = await profileService.GetContactMessagesAsync(user.Id);
        return View(messages);
    }

    [HttpPost("SendMessage")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(ContactMessageViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            return View("Messages", await profileService.GetContactMessagesAsync(user.Id));
        }

        var (success, message) = await profileService.SendContactMessageAsync(user.Id, model);
        if (!success)
        {
            TempData["ErrorMessage"] = message;
        }
        else
        {
            TempData["SuccessMessage"] = message;
        }

        return RedirectToAction("Messages");
    }
}
