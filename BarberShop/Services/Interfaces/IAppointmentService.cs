using BarberShop.Models;

namespace BarberShop.Services.Interfaces;

public interface IAppointmentService
{
    Task<bool> CreateAsync(string fullName, string email, string serviceName, string barberName, DateTime date, DateTime time, string? notes);
    Task<ProfileViewModel> GetProfileAsync(string email, int? selectedAppointmentId = null);
    Task<bool> UpdateAsync(int appointmentId, string email, string serviceName, string barberName, DateTime date, DateTime time, string? notes);
    Task<bool> DeleteAsync(int appointmentId, string email);
}
