using BarberShop.Models;
using BarberShop.Repositories.Interfaces;
using BarberShop.Services.Interfaces;
using BarberShop.Services.Email;

namespace BarberShop.Services;

public class AppointmentService(
    IAppointmentRepository appointmentRepository,
    IClientService clientService,
    IRepository<Client> clientRepository,
    IBarberService barberService,
    IServiceCatalogService serviceCatalogService,
    IEmailService emailService) : IAppointmentService
{
    public async Task<BookingViewModel> GetBookingAsync()
    {
        var services = await serviceCatalogService.GetAllAsync();
        var barbers = await barberService.GetAllAsync();

        return new BookingViewModel
        {
            AvailableServices = services.Where(s => s.IsActive).OrderBy(s => s.Name).Select(s => s.Name).ToList(),
            AvailableBarbers = barbers.Where(b => b.IsActive).OrderBy(b => b.FirstName).ThenBy(b => b.LastName).Select(b => $"{b.FirstName} {b.LastName}".Trim()).ToList()
        };
    }

    public async Task<bool> CreateAsync(BookingViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Service) || model.Date is null || model.Time is null)
            return false;

        var client = await clientService.GetOrCreateClientAsync(model.FullName, model.Email);
        var selectedService = await serviceCatalogService.ResolveServiceAsync(model.Service);
        var selectedBarber = await barberService.ResolveBarberAsync(model.Barber);

        // Defensive checks: ensure resolution succeeded
        if (client is null || selectedService is null || selectedBarber is null)
            return false;

        // Combine local date + time and convert to UTC for storage
        var localStart = model.Date.Value.Date + model.Time.Value;
        var localStartWithKind = DateTime.SpecifyKind(localStart, DateTimeKind.Local);
        var startDateTimeUtc = localStartWithKind.ToUniversalTime();
        var endDateTimeUtc = startDateTimeUtc.AddMinutes(selectedService.BaseDuration);

// Check for overlapping appointments for the same barber (exclude cancelled)
        var hasConflict = await appointmentRepository.HasOverlappingAppointmentAsync(selectedBarber.BarberId, startDateTimeUtc, endDateTimeUtc);
        if (hasConflict)
        {
            return false; // conflict
        }

        var appointment = new Appointment
        {
            ClientId = client.ClientId,
            BarberId = selectedBarber.BarberId,
            ServiceId = selectedService.ServiceId,
            StartDateTime = startDateTimeUtc,
            EndDateTime = endDateTimeUtc,
            Status = "Booked",
            Notes = model.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await appointmentRepository.AddAsync(appointment);

        // Send confirmation email asynchronously (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var localDisplay = startDateTimeUtc.ToLocalTime();
                var details = $"Your appointment is confirmed for {selectedService.Name} with {selectedBarber.FirstName} {selectedBarber.LastName} on {localDisplay:f}.";
                await emailService.SendAppointmentConfirmationAsync(client.Email, details);
            }
            catch { }
        });

        return true;
    }

    public async Task<List<Appointment>> GetUpcomingAsync(DateTime from, DateTime to)
    {
        return await appointmentRepository.GetUpcomingAsync(from, to);
    }

    public async Task<ProfileViewModel> GetProfileAsync(string email, int? selectedAppointmentId = null)
    {
        var normalizedEmail = (email ?? string.Empty).Trim();
        var client = await clientRepository.FirstOrDefaultAsync(c => c.Email == normalizedEmail);

        var allAppointments = client is null
            ? new List<Appointment>()
            : await appointmentRepository.GetByClientIdWithDetailsAsync(client.ClientId);

        var now = DateTime.UtcNow;
        var upcomingAppointments = allAppointments
            .Where(a => a.StartDateTime >= now && (a.Status == null || a.Status.ToLower() != "cancelled"))
            .ToList();

        var pastAppointments = allAppointments
            .Where(a => a.StartDateTime < now || (a.Status != null && a.Status.ToLower() == "cancelled"))
            .OrderByDescending(a => a.StartDateTime)
            .ToList();

        var selectedAppointment = selectedAppointmentId.HasValue
            ? allAppointments.FirstOrDefault(a => a.AppointmentId == selectedAppointmentId.Value)
            : upcomingAppointments.FirstOrDefault();

        var services = await serviceCatalogService.GetAllAsync();
        var barbers = await barberService.GetAllAsync();

        return new ProfileViewModel
        {
            FullName = client?.FullName ?? "Admin",
            Email = client?.Email ?? normalizedEmail,
            Phone = string.IsNullOrWhiteSpace(client?.Phone) ? "+407--------" : client.Phone,
            MemberSince = client?.MemberSince,
            UpcomingAppointments = upcomingAppointments,
            PastAppointments = pastAppointments,
            SelectedAppointment = selectedAppointment,
            AvailableServices = services.Where(s => s.IsActive).OrderBy(s => s.Name).Select(s => s.Name).ToList(),
            AvailableBarbers = barbers.Where(b => b.IsActive).OrderBy(b => b.FirstName).ThenBy(b => b.LastName).Select(b => $"{b.FirstName} {b.LastName}".Trim()).ToList()
        };
    }

    public async Task<bool> UpdateAsync(int appointmentId, string serviceName, string barberName, DateTime date, DateTime time, string? notes)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment is null)
            return false;

        var selectedService = await serviceCatalogService.ResolveServiceAsync(serviceName);
        var selectedBarber = await barberService.ResolveBarberAsync(barberName);

        if (selectedService is null || selectedBarber is null)
            return false;

        // Combine provided date + time as local, convert to UTC for storage
        var localStart = date.Date + time.TimeOfDay;
        var localStartWithKind = DateTime.SpecifyKind(localStart, DateTimeKind.Local);
        var startDateTimeUtc = localStartWithKind.ToUniversalTime();
        var endDateTimeUtc = startDateTimeUtc.AddMinutes(selectedService.BaseDuration);

        // Check for overlapping appointments, excluding the current appointment being updated
        var hasConflict = await appointmentRepository.HasOverlappingAppointmentAsync(
            selectedBarber.BarberId, startDateTimeUtc, endDateTimeUtc, appointmentId);
        if (hasConflict)
        {
            return false; // conflict detected
        }

        appointment.ServiceId = selectedService.ServiceId;
        appointment.BarberId = selectedBarber.BarberId;
        appointment.StartDateTime = startDateTimeUtc;
        appointment.EndDateTime = endDateTimeUtc;
        appointment.Notes = notes;
        appointment.Status = "Booked";

        await appointmentRepository.UpdateAsync(appointment);
        return true;
    }

    public async Task<bool> DeleteAsync(int appointmentId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment is null)
            return false;

        await appointmentRepository.DeleteAsync(appointment);
        return true;
    }
}
