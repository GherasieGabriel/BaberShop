using BarberShop.Models;
using ServiceEntity = BarberShop.Models.Service;

namespace BarberShop.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<List<ServiceEntity>> GetAllAsync();
    Task<ServiceEntity?> GetByIdAsync(int serviceId);
    Task<ServiceEntity?> GetByNameAsync(string name);
    Task<ServiceEntity> CreateAsync(ServiceEntity service);
    Task<bool> UpdateAsync(string originalName, ServiceEntity service);
    Task<bool> DeleteAsync(string name);
    Task<ServiceEntity> ResolveServiceAsync(string serviceName);
}
