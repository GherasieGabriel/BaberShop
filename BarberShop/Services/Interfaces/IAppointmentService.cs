using BarberShop.Models;

namespace BarberShop.Services.Interfaces;

public interface IAppointmentService
{
    Task<BookingViewModel> GetBookingAsync();
    Task<bool> CreateAsync(BookingViewModel model);
    Task<ProfileViewModel> GetProfileAsync(string email, int? selectedAppointmentId = null);
    Task<bool> UpdateAsync(int appointmentId, string serviceName, string barberName, DateTime date, DateTime time, string? notes);
    Task<bool> DeleteAsync(int appointmentId);
    Task<List<Appointment>> GetUpcomingAsync(DateTime from, DateTime to);
}
