using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService, IAdminAccessService adminAccessService) : ControllerBase
{
    [HttpGet("db-check")]
    public async Task<IActionResult> CheckDatabaseConnection()
    {
        try
        {
            await productService.GetAllAsync();
            return Ok(new { connected = true });
        }
        catch
        {
            return Ok(new { connected = false });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        return Ok(await productService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await productService.GetByIdAsync(id);
        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        if (!adminAccessService.IsAdmin())
            return Unauthorized(new { message = "Only admin can create products." });

        var created = await productService.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Product input)
    {
        if (!adminAccessService.IsAdmin())
            return Unauthorized(new { message = "Only admin can update products." });

        input.ProductId = id;
        var updated = await productService.UpdateAsync(input);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!adminAccessService.IsAdmin())
            return Unauthorized(new { message = "Only admin can delete products." });

        var deleted = await productService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
