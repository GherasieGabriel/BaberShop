using BarberShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarberShop.Models;

namespace BarberShop.Controllers;

[Authorize]
[Route("Orders")]
public class OrdersController(BarberShopDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var orders = await db.Orders
            .Where(o => o.ApplicationUserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .Include(o => o.Items)
            .ToListAsync();

        return View(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var order = await db.Orders
            .Where(o => o.OrderId == id && o.ApplicationUserId == user.Id)
            .Include(o => o.Items)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
}
