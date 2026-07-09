using BarberShop.Models;
using BarberShop.Repositories.Interfaces;
using BarberShop.Services;
using BarberShop.Services.Interfaces;
using Xunit;

namespace BarberShop.Tests;

public class CatalogAndIdentityServiceTests
{
    [Fact]
    public async Task ProductService_CreateAndLookup_Works()
    {
        var repository = new InMemoryRepository<Product>();
        var service = new ProductService(repository);

        var created = await service.CreateAsync(new Product { Name = "Pomade", Category = "Hair", Price = 45m, StockQuantity = 10, IsActive = true });
        var found = await service.GetByNameAsync("Pomade");

        Assert.Same(created, found);
        Assert.Equal("Pomade", found!.Name);
    }

    [Fact]
    public async Task ProductService_UpdateAndDelete_Works()
    {
        var existing = new Product { ProductId = 1, Name = "Shampoo", Category = "Hair", Price = 20m, StockQuantity = 5, IsActive = true };
        var repository = new InMemoryRepository<Product>(new[] { existing });
        var service = new ProductService(repository);

        var updated = await service.UpdateAsync("Shampoo", new Product { Name = "Shampoo Pro", Category = "Care", Price = 30m, StockQuantity = 3, IsActive = false });
        var deleted = await service.DeleteAsync("Shampoo Pro");

        Assert.True(updated);
        Assert.True(deleted);
        Assert.Empty(await repository.GetAllAsync());
    }

    [Fact]
    public async Task ServiceCatalogService_ResolveService_CreatesDefaultWhenMissing()
    {
        var repository = new InMemoryRepository<Service>();
        var service = new ServiceCatalogService(repository);

        var resolved = await service.ResolveServiceAsync("Beard Trim");

        Assert.Equal("Beard Trim", resolved.Name);
        Assert.Equal(30, resolved.BaseDuration);
        Assert.Equal(0, resolved.BasePrice);
        Assert.True(resolved.IsActive);
    }

    [Fact]
    public async Task ServiceCatalogService_UpdateAndDelete_Works()
    {
        var existing = new Service { ServiceId = 1, Name = "Haircut", BaseDuration = 30, BasePrice = 50m, IsActive = true };
        var repository = new InMemoryRepository<Service>(new[] { existing });
        var service = new ServiceCatalogService(repository);

        var updated = await service.UpdateAsync("Haircut", new Service { Name = "Premium Haircut", Description = "Longer service", BaseDuration = 45, BasePrice = 70m, IsActive = false });
        var deleted = await service.DeleteAsync("Premium Haircut");

        Assert.True(updated);
        Assert.True(deleted);
        Assert.Empty(await repository.GetAllAsync());
    }

    [Fact]
    public async Task BarberService_ResolveBarber_HandlesAnyAvailableAndCreation()
    {
        var activeBarber = new Barber { BarberId = 1, FirstName = "Alex", LastName = "Pop", Email = "alex@shop.com", Phone = "123", HireDate = DateTime.UtcNow.Date, IsActive = true };
        var repository = new InMemoryRepository<Barber>(new[] { activeBarber });
        var service = new BarberService(repository);

        var any = await service.ResolveBarberAsync("Any available");
        var created = await service.ResolveBarberAsync("John Doe");

        Assert.Same(activeBarber, any);
        Assert.Equal("John", created.FirstName);
        Assert.Equal("Doe", created.LastName);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task BarberService_UpdateAndDelete_Works()
    {
        var existing = new Barber { BarberId = 1, FirstName = "Mark", LastName = "Stone", Email = "mark@shop.com", Phone = "555", HireDate = DateTime.UtcNow.Date, IsActive = true };
        var repository = new InMemoryRepository<Barber>(new[] { existing });
        var service = new BarberService(repository);

        var updated = await service.UpdateAsync("mark@shop.com", new Barber { FirstName = "Marcus", LastName = "Stone", Email = "marcus@shop.com", Phone = "777", HireDate = DateTime.UtcNow.Date, IsActive = false });
        var deleted = await service.DeleteAsync("marcus@shop.com");

        Assert.True(updated);
        Assert.True(deleted);
        Assert.Empty(await repository.GetAllAsync());
    }

    [Fact]
    public async Task ClientService_ReturnsExistingOrCreatesNewClient()
    {
        var existing = new Client { ClientId = 1, FullName = "Jane Smith", Email = "jane@shop.com", Phone = "", MemberSince = DateTime.UtcNow.Date };
        var repository = new InMemoryRepository<Client>(new[] { existing });
        var service = new ClientService(repository);

        var found = await service.GetOrCreateClientAsync("  Jane Smith  ", "  jane@shop.com ");
        var created = await service.GetOrCreateClientAsync(" New Client ", "new@shop.com");

        Assert.Same(existing, found);
        Assert.Equal("New Client", created.FullName);
        Assert.Equal("new@shop.com", created.Email);
    }
}
