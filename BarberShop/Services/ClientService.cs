using BarberShop.Models;
using BarberShop.Repositories.Interfaces;
using BarberShop.Services.Interfaces;

namespace BarberShop.Services;

public class ClientService(IRepository<Client> clientRepository) : IClientService
{
    public async Task<Client> GetOrCreateClientAsync(string fullName, string email)
    {
        var normalizedEmail = (email ?? string.Empty).Trim();
        var client = await clientRepository.FirstOrDefaultAsync(c => c.Email == normalizedEmail);
        if (client is not null)
            return client;

        client = new Client
        {
            FullName = (fullName ?? string.Empty).Trim(),
            Email = normalizedEmail,
            Phone = string.Empty,
            MemberSince = DateTime.UtcNow.Date
        };

        return await clientRepository.AddAsync(client);
    }
}
