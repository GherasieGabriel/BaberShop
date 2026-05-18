using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

[Authorize]
public class AppointmentsController(
    IAppointmentService appointmentService,
    IServiceCatalogService serviceCatalogService,
    IBarberService barberService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Booking()
    {
        return View(await appointmentService.GetBookingAsync());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Booking(BookingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var vm = await appointmentService.GetBookingAsync();
            vm.FullName = model.FullName;
            vm.Email = model.Email;
            vm.Service = model.Service;
            vm.Barber = model.Barber;
            vm.Date = model.Date;
            vm.Time = model.Time;
            vm.Notes = model.Notes;
            return View(vm);
        }

        var created = await appointmentService.CreateAsync(model);
        TempData[created ? "BookingSuccess" : "BookingError"] = created
            ? "Your appointment was saved."
            : "Please complete all required fields.";

        return RedirectToAction(nameof(Booking));
    }

    [HttpGet]
    public async Task<IActionResult> Profile(int? selectedAppointmentId = null)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await appointmentService.GetProfileAsync(user.Email ?? string.Empty, selectedAppointmentId);
        model.UserId = user.Id;
        model.ProfileImage = user.ProfileImage;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // This shows the user's appointments
        // We can refactor this later to use Identity user
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAppointment(int appointmentId, string service, string barber, DateTime date, DateTime time, string? notes)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var updated = await appointmentService.UpdateAsync(appointmentId, service, barber, date, time, notes);
        TempData[updated ? "BookingSuccess" : "BookingError"] = updated ? "Appointment updated." : "Appointment not found.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAppointment(int appointmentId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var deleted = await appointmentService.DeleteAsync(appointmentId);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Appointment deleted." : "Appointment not found.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (profileImage != null && profileImage.Length > 0)
        {
            // Limit file size to 5MB
            if (profileImage.Length > 5 * 1024 * 1024)
            {
                TempData["BookingError"] = "Image must be smaller than 5MB.";
                return RedirectToAction(nameof(Profile));
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(profileImage.ContentType))
            {
                TempData["BookingError"] = "Only image files (JPEG, PNG, GIF, WebP) are allowed.";
                return RedirectToAction(nameof(Profile));
            }

            using (var memoryStream = new MemoryStream())
            {
                await profileImage.CopyToAsync(memoryStream);
                user.ProfileImage = memoryStream.ToArray();
            }

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["BookingSuccess"] = "Profile image updated successfully.";
            }
            else
            {
                TempData["BookingError"] = "Failed to update profile image.";
            }
        }

        return RedirectToAction(nameof(Profile));
    }
}