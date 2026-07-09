using BarberShop.Models;

namespace BarberShop.Repositories.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetByClientIdWithDetailsAsync(int clientId);
    Task<Appointment?> GetByIdAsync(int appointmentId);
    Task<Appointment?> GetByIdForClientEmailAsync(int appointmentId, string email);
    Task<Appointment?> GetByClientEmailAndStartAsync(string email, DateTime startDateTime);
    Task<Appointment> AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(Appointment appointment);
    Task<bool> HasOverlappingAppointmentAsync(int barberId, DateTime start, DateTime end);
    Task<bool> HasOverlappingAppointmentAsync(int barberId, DateTime start, DateTime end, int? excludeAppointmentId);
    Task<List<Appointment>> GetUpcomingAsync(DateTime from, DateTime to);
}
