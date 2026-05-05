using BarberShop.Repositories.Interfaces;
using BarberShop.Services.Interfaces;
using ServiceEntity = BarberShop.Models.Service;

namespace BarberShop.Services;

public class ServiceCatalogService(IRepository<ServiceEntity> serviceRepository) : IServiceCatalogService
{
    public Task<List<ServiceEntity>> GetAllAsync()
        => serviceRepository.GetAllAsync();

    public Task<ServiceEntity?> GetByIdAsync(int serviceId)
        => serviceRepository.GetByIdAsync(serviceId);

    public Task<ServiceEntity?> GetByNameAsync(string name)
        => serviceRepository.FirstOrDefaultAsync(s => s.Name == name);

    public Task<ServiceEntity> CreateAsync(ServiceEntity service)
        => serviceRepository.AddAsync(service);

    public async Task<bool> UpdateAsync(string originalName, ServiceEntity service)
    {
        var existing = await serviceRepository.FirstOrDefaultAsync(s => s.Name == originalName);
        if (existing is null)
            return false;

        existing.Name = service.Name;
        existing.Description = service.Description;
        existing.BaseDuration = service.BaseDuration;
        existing.BasePrice = service.BasePrice;
        existing.IsActive = service.IsActive;

        await serviceRepository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(string name)
    {
        var service = await serviceRepository.FirstOrDefaultAsync(s => s.Name == name);
        if (service is null)
            return false;

        await serviceRepository.DeleteAsync(service);
        return true;
    }

    public async Task<ServiceEntity> ResolveServiceAsync(string serviceName)
    {
        var service = await serviceRepository.FirstOrDefaultAsync(s => s.Name == serviceName);
        if (service is not null)
            return service;

        service = new ServiceEntity
        {
            Name = serviceName,
            BaseDuration = 30,
            BasePrice = 0,
            IsActive = true
        };

        return await serviceRepository.AddAsync(service);
    }
}
