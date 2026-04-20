using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BarberShop.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    IAppointmentService appointmentService,
    IBarberService barberService,
    IServiceCatalogService serviceCatalogService,
    IProductService productService) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Services()
    {
        var services = await serviceCatalogService.GetAllAsync();
        return View(services);
    }

    [HttpGet]
    public async Task<IActionResult> Barbers()
    {
        var barbers = await barberService.GetAllAsync();
        return View(barbers);
    }

    [HttpGet]
    public async Task<IActionResult> Products()
    {
        var products = await productService.GetAllAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Profile(string email = "admin@yahoo.com", int? selectedAppointmentId = null)
    {
        var model = await appointmentService.GetProfileAsync(email, selectedAppointmentId);
        return View(model);
    }

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

    public IActionResult Prices()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Gallery()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
