using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

public class ProductsPageController(
    IProductService productService,
    IAdminAccessService adminAccessService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var products = await productService.GetAllAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can manage products.";
            return RedirectToAction("Index", "Home");
        }

        var products = await productService.GetAllAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can edit products.";
            return RedirectToAction("Index", "Home");
        }

        var product = await productService.GetByIdAsync(id);
        if (product is null)
        {
            TempData["BookingError"] = "Product not found.";
            return RedirectToAction(nameof(Manage));
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, string category, decimal price, int stockQuantity, bool isActive)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can create products.";
            return RedirectToAction(nameof(Manage));
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category) || price < 0 || stockQuantity < 0)
        {
            TempData["BookingError"] = "Invalid product input.";
            return RedirectToAction(nameof(Manage));
        }

        await productService.CreateAsync(new Product
        {
            Name = name.Trim(),
            Description = description,
            Category = category.Trim(),
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = isActive
        });

        TempData["BookingSuccess"] = "Product created.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, string name, string? description, string category, decimal price, int stockQuantity, bool isActive)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can update products.";
            return RedirectToAction(nameof(Manage));
        }

        var updated = await productService.UpdateAsync(new Product
        {
            ProductId = productId,
            Name = name.Trim(),
            Description = description,
            Category = category.Trim(),
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = isActive
        });

        TempData[updated ? "BookingSuccess" : "BookingError"] = updated ? "Product updated." : "Product not found.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int productId)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can delete products.";
            return RedirectToAction(nameof(Manage));
        }

        var deleted = await productService.DeleteAsync(productId);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Product deleted." : "Product not found.";
        return RedirectToAction(nameof(Manage));
    }
}
