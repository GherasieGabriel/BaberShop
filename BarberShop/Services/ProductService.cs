using BarberShop.Models;
using BarberShop.Repositories.Interfaces;
using BarberShop.Services.Interfaces;

namespace BarberShop.Services;

public class ProductService(IRepository<Product> productRepository) : IProductService
{
    public Task<List<Product>> GetAllAsync()
        => productRepository.GetAllAsync();

    public Task<Product?> GetByIdAsync(int productId)
        => productRepository.GetByIdAsync(productId);

    public Task<Product> CreateAsync(Product product)
        => productRepository.AddAsync(product);

    public async Task<bool> UpdateAsync(Product product)
    {
        var existing = await productRepository.GetByIdAsync(product.ProductId);
        if (existing is null)
            return false;

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Category = product.Category;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;
        existing.IsActive = product.IsActive;

        await productRepository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int productId)
    {
        var product = await productRepository.GetByIdAsync(productId);
        if (product is null)
            return false;

        await productRepository.DeleteAsync(product);
        return true;
    }
}
