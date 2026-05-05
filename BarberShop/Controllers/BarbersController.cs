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

        var viewModel = new ManageBarbersViewModel
        {
            Barbers = await barberService.GetAllAsync()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string email)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can edit barbers.";
            return RedirectToAction(nameof(Index));
        }

        var barber = await barberService.GetByEmailAsync(email);
        if (barber is null)
        {
            TempData["BookingError"] = "Barber not found.";
            return RedirectToAction(nameof(Manage));
        }

        return View(barber);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Barber model)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can create barbers.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            TempData["BookingError"] = "Invalid barber input.";
            return RedirectToAction(nameof(Manage));
        }

        model.FirstName = model.FirstName.Trim();
        model.LastName = model.LastName.Trim();
        model.Phone = model.Phone?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
        model.HireDate = DateTime.UtcNow.Date;
        await barberService.CreateAsync(model);

        TempData["BookingSuccess"] = "Barber created successfully.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string originalEmail, string firstName, string lastName, string? phone, string? email, bool isActive)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can update barbers.";
            return RedirectToAction(nameof(Index));
        }

        var updated = await barberService.UpdateAsync(originalEmail, new Barber
        {
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
    public async Task<IActionResult> Delete(string email)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can delete barbers.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = await barberService.DeleteAsync(email);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Barber deleted." : "Barber not found.";
        return RedirectToAction(nameof(Manage));
    }
}
