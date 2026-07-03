using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

public class BarbersController(IBarberService barberService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var barbers = await barberService.GetAllAsync();
        return View(barbers);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(new List<object>());

        var all = await barberService.GetAllAsync();
        var lowered = q.Trim().ToLowerInvariant();
        var matches = all.Where(b => ($"{b.FirstName} {b.LastName}".ToLowerInvariant().Contains(lowered) || b.Email.ToLowerInvariant().Contains(lowered)))
            .Select(b => new { id = b.BarberId, name = $"{b.FirstName} {b.LastName}", email = b.Email, phone = b.Phone })
            .ToList();

        return Json(matches);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Manage()
    {
        var viewModel = new ManageBarbersViewModel
        {
            Barbers = await barberService.GetAllAsync()
        };
        return View(viewModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string email)
    {
        var barber = await barberService.GetByEmailAsync(email);
        if (barber is null)
        {
            TempData["BookingError"] = "Barber not found.";
            return RedirectToAction(nameof(Manage));
        }
        return View(barber);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Barber model)
    {
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
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string originalEmail, string firstName, string lastName, string? phone, string? email, bool isActive)
    {
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
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string email)
    {
        var deleted = await barberService.DeleteAsync(email);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Barber deleted." : "Barber not found.";
        return RedirectToAction(nameof(Manage));
    }
}
