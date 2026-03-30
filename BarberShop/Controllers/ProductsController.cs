using BarberShop.Data;
using BarberShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(BarberShopDbContext context) : ControllerBase
{
    [HttpGet("db-check")]
    public async Task<IActionResult> CheckDatabaseConnection()
    {
        var canConnect = await context.Database.CanConnectAsync();
        return Ok(new { connected = canConnect });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        return Ok(await context.Products.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Product input)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        product.Name = input.Name;
        product.Description = input.Description;
        product.Category = input.Category;
        product.Price = input.Price;
        product.StockQuantity = input.StockQuantity;
        product.IsActive = input.IsActive;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
