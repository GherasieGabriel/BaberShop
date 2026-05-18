using BarberShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

[Route("Cart")]
public class CartController(ICartService cartService) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        var cart = cartService.GetCart(HttpContext);
        return View(cart);
    }

    [HttpPost("Add")]
    [ValidateAntiForgeryToken]
    public IActionResult Add(string id, string name, string price, int quantity = 1)
    {
        if (decimal.TryParse(price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedPrice))
        {
            cartService.AddToCart(HttpContext, id, name, parsedPrice, quantity);
            TempData["SuccessMessage"] = $"Added {name} to cart";
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        TempData["ErrorMessage"] = "Invalid price format";
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    [HttpPost("Remove")]
    public IActionResult Remove(string id)
    {
        cartService.RemoveFromCart(HttpContext, id);
        TempData["SuccessMessage"] = "Item removed from cart";
        return RedirectToAction("Index");
    }

    [HttpPost("Update")]
    public IActionResult Update(string id, int quantity)
    {
        if (quantity <= 0)
        {
            cartService.RemoveFromCart(HttpContext, id);
        }
        else
        {
            cartService.UpdateQuantity(HttpContext, id, quantity);
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Clear")]
    public IActionResult Clear()
    {
        cartService.ClearCart(HttpContext);
        TempData["SuccessMessage"] = "Cart cleared";
        return RedirectToAction("Index");
    }
}
