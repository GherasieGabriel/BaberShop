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

        var viewModel = new ManageServicesViewModel
        {
            Services = await serviceCatalogService.GetAllAsync()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string name)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can edit services.";
            return RedirectToAction(nameof(Index));
        }

        var service = await serviceCatalogService.GetByNameAsync(name);
        if (service is null)
        {
            TempData["BookingError"] = "Service not found.";
            return RedirectToAction(nameof(Manage));
        }

        return View(service);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Service model)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can create services.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            TempData["BookingError"] = "Invalid service input.";
            return RedirectToAction(nameof(Manage));
        }

        model.Name = model.Name.Trim();
        await serviceCatalogService.CreateAsync(model);

        TempData["BookingSuccess"] = "Service created.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string originalName, string name, string? description, int baseDuration, decimal basePrice, bool isActive)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can update services.";
            return RedirectToAction(nameof(Index));
        }

        var updated = await serviceCatalogService.UpdateAsync(originalName, new Service
        {
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
    public async Task<IActionResult> Delete(string name)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can delete services.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = await serviceCatalogService.DeleteAsync(name);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Service deleted." : "Service not found.";
        return RedirectToAction(nameof(Manage));
    }
}
