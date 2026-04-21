using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

public class AdminController(IAdminAccessService adminAccessService) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        if (adminAccessService.IsAdmin())
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string username, string password)
    {
        if (!adminAccessService.TryLogin(username, password))
        {
            TempData["BookingError"] = "Invalid admin credentials.";
            return RedirectToAction(nameof(Login));
        }

        TempData["BookingSuccess"] = "Admin mode enabled.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        adminAccessService.Logout();
        TempData["BookingSuccess"] = "Admin mode disabled.";
        return RedirectToAction("Index", "Home");
    }
}
