using BarberShop.Models;

namespace BarberShop.Services.Interfaces;

public interface IClientService
{
    Task<Client> GetOrCreateClientAsync(string fullName, string email);
}
