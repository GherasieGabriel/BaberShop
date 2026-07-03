using BarberShop.Models;
using BarberShop.Services.Interfaces;
using BarberShop.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly IAppointmentService appointmentService;
    private readonly IServiceCatalogService serviceCatalogService;
    private readonly IBarberService barberService;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IAppointmentRepository appointmentRepository;

    public AppointmentsController(IAppointmentService appointmentService, IServiceCatalogService serviceCatalogService, IBarberService barberService, UserManager<ApplicationUser> userManager, IAppointmentRepository appointmentRepository)
    {
        this.appointmentService = appointmentService;
        this.serviceCatalogService = serviceCatalogService;
        this.barberService = barberService;
        this.userManager = userManager;
        this.appointmentRepository = appointmentRepository;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Booking(string? date, string? time)
    {
        var vm = await appointmentService.GetBookingAsync();
        // Prefill date/time if provided as query parameters (date=YYYY-MM-DD, time=HH:mm)
        if (!string.IsNullOrWhiteSpace(date))
        {
            if (DateTime.TryParse(date, out var parsedDate))
            {
                vm.Date = parsedDate.Date;
            }
        }
        if (!string.IsNullOrWhiteSpace(time))
        {
            if (TimeSpan.TryParse(time, out var parsedTime))
            {
                vm.Time = parsedTime;
            }
        }
        return View(vm);
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
        if (!created)
        {
            TempData["BookingError"] = "Please complete all required fields or choose another time slot.";
            return RedirectToAction(nameof(Booking));
        }

        var selectedStart = DateTime.SpecifyKind(model.Date!.Value.Date + model.Time!.Value, DateTimeKind.Local).ToUniversalTime();
        var appointment = await appointmentRepository.GetByClientEmailAndStartAsync(model.Email.Trim(), selectedStart);
        if (appointment is null)
        {
            TempData["BookingSuccess"] = "Your appointment was saved.";
            return RedirectToAction(nameof(Booking));
        }

        return RedirectToAction(nameof(Confirmation), new { appointmentId = appointment.AppointmentId });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Confirmation(int appointmentId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment is null)
        {
            return NotFound();
        }

        return View(new AppointmentConfirmationViewModel
        {
            Appointment = appointment
        });
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

    // API endpoint for calendar to retrieve appointments (JSON)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAppointmentsJson()
    {
        var now = DateTime.UtcNow;
        var to = now.AddDays(60);
        var items = await appointmentRepository.GetUpcomingAsync(now, to);
        var events = items.Select(a => new {
            id = a.AppointmentId,
            title = $"{a.Service.Name} - {a.Client.FullName}",
            start = a.StartDateTime.ToString("o"),
            end = a.EndDateTime.ToString("o"),
            barber = $"{a.Barber.FirstName} {a.Barber.LastName}",
            status = a.Status
        });
        return Json(events);
    }

    // API endpoint to create appointment via JSON (used by calendar UI)
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAppointmentApi([FromBody] BookingViewModel model)
    {
        if (model == null)
            return BadRequest(new { success = false, message = "Invalid payload" });

        var created = await appointmentService.CreateAsync(model);
        if (!created)
            return Conflict(new { success = false, message = "Time slot unavailable or invalid data" });

        return Ok(new { success = true });
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