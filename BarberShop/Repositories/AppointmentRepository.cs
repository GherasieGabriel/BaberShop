using BarberShop.Data;
using BarberShop.Models;
using BarberShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Repositories;

public class AppointmentRepository(BarberShopDbContext context) : IAppointmentRepository
{
    public Task<List<Appointment>> GetByClientIdWithDetailsAsync(int clientId)
        => context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Barber)
            .Where(a => a.ClientId == clientId)
            .OrderBy(a => a.StartDateTime)
            .ToListAsync();

    public Task<Appointment?> GetByIdAsync(int appointmentId)
        => context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Include(a => a.Barber)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

    public Task<Appointment?> GetByIdForClientEmailAsync(int appointmentId, string email)
        => context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Include(a => a.Barber)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.Client.Email == email);

    public Task<Appointment?> GetByClientEmailAndStartAsync(string email, DateTime startDateTime)
        => context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Include(a => a.Barber)
            .FirstOrDefaultAsync(a => a.Client.Email == email && a.StartDateTime == startDateTime);

    public Task<List<Appointment>> GetUpcomingAsync(DateTime from, DateTime to)
        => context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Include(a => a.Barber)
            .Where(a => a.StartDateTime >= from && a.StartDateTime <= to && (a.Status == null || a.Status.ToLower() != "cancelled"))
            .OrderBy(a => a.StartDateTime)
            .ToListAsync();

    public async Task<Appointment> AddAsync(Appointment appointment)
    {
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment;
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        context.Appointments.Update(appointment);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Appointment appointment)
    {
        context.Appointments.Remove(appointment);
        await context.SaveChangesAsync();
    }

    public Task<bool> HasOverlappingAppointmentAsync(int barberId, DateTime start, DateTime end)
        => context.Appointments
            .AnyAsync(a => a.BarberId == barberId && (a.Status == null || a.Status.ToLower() != "cancelled") && a.StartDateTime < end && a.EndDateTime > start);

    public Task<bool> HasOverlappingAppointmentAsync(int barberId, DateTime start, DateTime end, int? excludeAppointmentId)
        => context.Appointments
            .AnyAsync(a => a.BarberId == barberId && (!excludeAppointmentId.HasValue || a.AppointmentId != excludeAppointmentId.Value) && (a.Status == null || a.Status.ToLower() != "cancelled") && a.StartDateTime < end && a.EndDateTime > start);
}
