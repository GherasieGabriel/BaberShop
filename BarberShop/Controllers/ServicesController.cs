using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

public class ServicesController(
    IServiceCatalogService serviceCatalogService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var services = await serviceCatalogService.GetAllAsync();
        return View(services);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Manage()
    {
        var viewModel = new ManageServicesViewModel
        {
            Services = await serviceCatalogService.GetAllAsync()
        };

        return View(viewModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string name)
    {
        var service = await serviceCatalogService.GetByNameAsync(name);
        if (service is null)
        {
            TempData["BookingError"] = "Service not found.";
            return RedirectToAction(nameof(Manage));
        }

        return View(service);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Service model)
    {
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
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string originalName, string name, string? description, int baseDuration, decimal basePrice, bool isActive)
    {
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
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string name)
    {
        var deleted = await serviceCatalogService.DeleteAsync(name);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Service deleted." : "Service not found.";
        return RedirectToAction(nameof(Manage));
    }
}
