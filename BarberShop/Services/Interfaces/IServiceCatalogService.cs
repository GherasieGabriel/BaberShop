using BarberShop.Models;
using ServiceEntity = BarberShop.Models.Service;

namespace BarberShop.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<List<ServiceEntity>> GetAllAsync();
    Task<ServiceEntity?> GetByIdAsync(int serviceId);
    Task<ServiceEntity> CreateAsync(ServiceEntity service);
    Task<bool> UpdateAsync(ServiceEntity service);
    Task<bool> DeleteAsync(int serviceId);
    Task<ServiceEntity> ResolveServiceAsync(string serviceName);
}
