using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

public class ServicesController(
    IServiceCatalogService serviceCatalogService,
    IAdminAccessService adminAccessService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var services = await serviceCatalogService.GetAllAsync();
        return View(services);
    }

    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can manage services.";
            return RedirectToAction(nameof(Index));
        }

        var services = await serviceCatalogService.GetAllAsync();
        return View(services);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can edit services.";
            return RedirectToAction(nameof(Index));
        }

        var service = await serviceCatalogService.GetByIdAsync(id);
        if (service is null)
        {
            TempData["BookingError"] = "Service not found.";
            return RedirectToAction(nameof(Manage));
        }

        return View(service);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, int baseDuration, decimal basePrice, bool isActive)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can create services.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(name) || baseDuration <= 0 || basePrice < 0)
        {
            TempData["BookingError"] = "Invalid service input.";
            return RedirectToAction(nameof(Manage));
        }

        await serviceCatalogService.CreateAsync(new Service
        {
            Name = name.Trim(),
            Description = description,
            BaseDuration = baseDuration,
            BasePrice = basePrice,
            IsActive = isActive
        });

        TempData["BookingSuccess"] = "Service created.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int serviceId, string name, string? description, int baseDuration, decimal basePrice, bool isActive)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can update services.";
            return RedirectToAction(nameof(Index));
        }

        var updated = await serviceCatalogService.UpdateAsync(new Service
        {
            ServiceId = serviceId,
            Name = name.Trim(),
            Description = description,
            BaseDuration = baseDuration,
            BasePrice = basePrice,
            IsActive = isActive
        });

        TempData[updated ? "BookingSuccess" : "BookingError"] = updated ? "Service updated." : "Service not found.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int serviceId)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can delete services.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = await serviceCatalogService.DeleteAsync(serviceId);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Service deleted." : "Service not found.";
        return RedirectToAction(nameof(Manage));
    }
}
