using BarberShop.Models;

namespace BarberShop.Services.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int productId);
    Task<Product?> GetByNameAsync(string name);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(string originalName, Product product);
    Task<bool> DeleteAsync(string name);
}
