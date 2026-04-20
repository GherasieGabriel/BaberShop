using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

public class BarbersController(
    IBarberService barberService,
    IAdminAccessService adminAccessService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var barbers = await barberService.GetAllAsync();
        return View(barbers);
    }

    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can manage barbers.";
            return RedirectToAction(nameof(Index));
        }

        var barbers = await barberService.GetAllAsync();
        return View(barbers);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can edit barbers.";
            return RedirectToAction(nameof(Index));
        }

        var barber = await barberService.GetByIdAsync(id);
        if (barber is null)
        {
            TempData["BookingError"] = "Barber not found.";
            return RedirectToAction(nameof(Manage));
        }

        return View(barber);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string firstName, string lastName, string? phone, string? email, bool isActive)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can create barbers.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            TempData["BookingError"] = "First and last names are required.";
            return RedirectToAction(nameof(Manage));
        }

        await barberService.CreateAsync(new Barber
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Phone = phone?.Trim() ?? string.Empty,
            Email = email?.Trim() ?? string.Empty,
            HireDate = DateTime.UtcNow.Date,
            IsActive = isActive
        });

        TempData["BookingSuccess"] = "Barber created successfully.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int barberId, string firstName, string lastName, string? phone, string? email, bool isActive)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can update barbers.";
            return RedirectToAction(nameof(Index));
        }

        var updated = await barberService.UpdateAsync(new Barber
        {
            BarberId = barberId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Phone = phone?.Trim() ?? string.Empty,
            Email = email?.Trim() ?? string.Empty,
            HireDate = DateTime.UtcNow.Date,
            IsActive = isActive
        });

        TempData[updated ? "BookingSuccess" : "BookingError"] = updated ? "Barber updated." : "Barber not found.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int barberId)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can delete barbers.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = await barberService.DeleteAsync(barberId);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Barber deleted." : "Barber not found.";
        return RedirectToAction(nameof(Manage));
    }
}
