using BarberShop.Models;
using BarberShop.Repositories.Interfaces;
using BarberShop.Services.Interfaces;

namespace BarberShop.Services;

public class BarberService(IRepository<Barber> barberRepository) : IBarberService
{
    public Task<List<Barber>> GetAllAsync()
        => barberRepository.GetAllAsync();

    public Task<Barber?> GetByIdAsync(int barberId)
        => barberRepository.GetByIdAsync(barberId);

    public Task<Barber> CreateAsync(Barber barber)
        => barberRepository.AddAsync(barber);

    public async Task<bool> UpdateAsync(Barber barber)
    {
        var existing = await barberRepository.GetByIdAsync(barber.BarberId);
        if (existing is null)
            return false;

        existing.FirstName = barber.FirstName;
        existing.LastName = barber.LastName;
        existing.Email = barber.Email;
        existing.Phone = barber.Phone;
        existing.HireDate = barber.HireDate;
        existing.IsActive = barber.IsActive;

        await barberRepository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int barberId)
    {
        var barber = await barberRepository.GetByIdAsync(barberId);
        if (barber is null)
            return false;

        await barberRepository.DeleteAsync(barber);
        return true;
    }

    public async Task<Barber> ResolveBarberAsync(string barberName)
    {
        Barber? barber;
        if (string.Equals(barberName, "Any available", StringComparison.OrdinalIgnoreCase))
        {
            barber = await barberRepository.FirstOrDefaultAsync(b => b.IsActive);
        }
        else
        {
            barber = await barberRepository.FirstOrDefaultAsync(b => b.FirstName == barberName && b.IsActive);
        }

        if (barber is not null)
            return barber;

        barber = new Barber
        {
            FirstName = string.Equals(barberName, "Any available", StringComparison.OrdinalIgnoreCase) ? "Any" : barberName,
            LastName = "Available",
            Phone = string.Empty,
            Email = string.Empty,
            HireDate = DateTime.UtcNow.Date,
            IsActive = true
        };

        return await barberRepository.AddAsync(barber);
    }
}
