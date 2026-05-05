using BarberShop.Models;

namespace BarberShop.Services.Interfaces;

public interface IBarberService
{
    Task<List<Barber>> GetAllAsync();
    Task<Barber?> GetByIdAsync(int barberId);
    Task<Barber?> GetByEmailAsync(string email);
    Task<Barber?> GetByFullNameAsync(string firstName, string lastName);
    Task<Barber> CreateAsync(Barber barber);
    Task<bool> UpdateAsync(string originalEmail, Barber barber);
    Task<bool> DeleteAsync(string email);
    Task<Barber> ResolveBarberAsync(string barberName);
}
