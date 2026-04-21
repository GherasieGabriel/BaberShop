using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

public class AppointmentsController(
    IAppointmentService appointmentService,
    IServiceCatalogService serviceCatalogService,
    IBarberService barberService) : Controller
{
    [HttpGet]
    public IActionResult Booking()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Booking(string fullName, string email, string service, string barber, DateTime date, DateTime time, string? notes)
    {
        var created = await appointmentService.CreateAsync(fullName, email, service, barber, date, time, notes);
        TempData[created ? "BookingSuccess" : "BookingError"] = created
            ? "Your appointment was saved."
            : "Please complete all required fields.";

        return RedirectToAction(nameof(Booking));
    }

    [HttpGet]
    public async Task<IActionResult> Profile(string email = "admin@yahoo.com", int? selectedAppointmentId = null)
    {
        var model = await appointmentService.GetProfileAsync(email, selectedAppointmentId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAppointment(int appointmentId, string email, string service, string barber, DateTime date, DateTime time, string? notes)
    {
        var updated = await appointmentService.UpdateAsync(appointmentId, email, service, barber, date, time, notes);
        TempData[updated ? "BookingSuccess" : "BookingError"] = updated ? "Appointment updated." : "Appointment not found.";
        return RedirectToAction(nameof(Profile), new { email, selectedAppointmentId = appointmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAppointment(int appointmentId, string email)
    {
        var deleted = await appointmentService.DeleteAsync(appointmentId, email);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Appointment deleted." : "Appointment not found.";
        return RedirectToAction(nameof(Profile), new { email });
    }
}