using BarberShop.Models;

namespace BarberShop.Services.Interfaces;

public interface IAppointmentService
{
    Task<BookingViewModel> GetBookingAsync();
    Task<bool> CreateAsync(BookingViewModel model);
    Task<ProfileViewModel> GetProfileAsync(string email, int? selectedAppointmentId = null);
    Task<bool> UpdateAsync(string email, DateTime appointmentStartDateTime, string serviceName, string barberName, DateTime date, DateTime time, string? notes);
    Task<bool> DeleteAsync(string email, DateTime appointmentStartDateTime);
}
