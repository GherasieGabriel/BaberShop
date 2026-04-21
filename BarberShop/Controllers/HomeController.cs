using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BarberShop.Controllers;

public class HomeController(IServiceCatalogService serviceCatalogService) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Prices()
    {
        var services = await serviceCatalogService.GetAllAsync();
        return View(services);
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
