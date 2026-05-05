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
    public async Task<IActionResult> Booking()
    {
        return View(await appointmentService.GetBookingAsync());
    }

    [HttpPost]
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
    public async Task<IActionResult> Profile(string email = "admin@yahoo.com", int? selectedAppointmentId = null)
    {
        var model = await appointmentService.GetProfileAsync(email, selectedAppointmentId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAppointment(string email, DateTime appointmentStartDateTime, string service, string barber, DateTime date, DateTime time, string? notes)
    {
        var updated = await appointmentService.UpdateAsync(email, appointmentStartDateTime, service, barber, date, time, notes);
        TempData[updated ? "BookingSuccess" : "BookingError"] = updated ? "Appointment updated." : "Appointment not found.";
        return RedirectToAction(nameof(Profile), new { email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAppointment(string email, DateTime appointmentStartDateTime)
    {
        var deleted = await appointmentService.DeleteAsync(email, appointmentStartDateTime);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Appointment deleted." : "Appointment not found.";
        return RedirectToAction(nameof(Profile), new { email });
    }
}