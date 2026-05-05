using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

public class ProductsController(
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
            return RedirectToAction("Index");
        }

        var viewModel = new ManageProductsViewModel
        {
            Products = await productService.GetAllAsync()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string name)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can edit products.";
            return RedirectToAction(nameof(Index));
        }

        var product = await productService.GetByNameAsync(name);
        if (product is null)
        {
            TempData["BookingError"] = "Product not found.";
            return RedirectToAction(nameof(Manage));
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can create products.";
            return RedirectToAction(nameof(Manage));
        }

        if (!ModelState.IsValid)
        {
            TempData["BookingError"] = "Invalid product input.";
            return RedirectToAction(nameof(Manage));
        }

        model.Name = model.Name.Trim();
        model.Category = model.Category.Trim();
        await productService.CreateAsync(model);

        TempData["BookingSuccess"] = "Product created.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string originalName, string name, string? description, string category, decimal price, int stockQuantity, bool isActive)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can update products.";
            return RedirectToAction(nameof(Manage));
        }

        var updated = await productService.UpdateAsync(originalName, new Product
        {
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
    public async Task<IActionResult> Delete(string name)
    {
        if (!adminAccessService.IsAdmin())
        {
            TempData["BookingError"] = "Only admin can delete products.";
            return RedirectToAction(nameof(Manage));
        }

        var deleted = await productService.DeleteAsync(name);
        TempData[deleted ? "BookingSuccess" : "BookingError"] = deleted ? "Product deleted." : "Product not found.";
        return RedirectToAction(nameof(Manage));
    }
}
