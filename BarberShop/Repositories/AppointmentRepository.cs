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
}
