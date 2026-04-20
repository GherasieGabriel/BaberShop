using BarberShop.Models;

namespace BarberShop.Services.Interfaces;

public interface IBarberService
{
    Task<List<Barber>> GetAllAsync();
    Task<Barber?> GetByIdAsync(int barberId);
    Task<Barber> CreateAsync(Barber barber);
    Task<bool> UpdateAsync(Barber barber);
    Task<bool> DeleteAsync(int barberId);
    Task<Barber> ResolveBarberAsync(string barberName);
}
