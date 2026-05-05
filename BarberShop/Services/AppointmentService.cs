using BarberShop.Models;
using BarberShop.Repositories.Interfaces;
using BarberShop.Services.Interfaces;

namespace BarberShop.Services;

public class AppointmentService(
    IAppointmentRepository appointmentRepository,
    IClientService clientService,
    IRepository<Client> clientRepository,
    IBarberService barberService,
    IServiceCatalogService serviceCatalogService) : IAppointmentService
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

        var startDateTime = model.Date.Value.Date + model.Time.Value;
        var appointment = new Appointment
        {
            ClientId = client.ClientId,
            BarberId = selectedBarber.BarberId,
            ServiceId = selectedService.ServiceId,
            StartDateTime = startDateTime,
            EndDateTime = startDateTime.AddMinutes(selectedService.BaseDuration),
            Status = "Booked",
            Notes = model.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await appointmentRepository.AddAsync(appointment);
        return true;
    }

    public async Task<ProfileViewModel> GetProfileAsync(string email, int? selectedAppointmentId = null)
    {
        var normalizedEmail = (email ?? string.Empty).Trim();
        var client = await clientRepository.FirstOrDefaultAsync(c => c.Email == normalizedEmail);

        var allAppointments = client is null
            ? new List<Appointment>()
            : await appointmentRepository.GetByClientIdWithDetailsAsync(client.ClientId);

        var now = DateTime.Now;
        var upcomingAppointments = allAppointments
            .Where(a => a.StartDateTime >= now && !string.Equals(a.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var pastAppointments = allAppointments
            .Where(a => a.StartDateTime < now || string.Equals(a.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
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

    public async Task<bool> UpdateAsync(string email, DateTime appointmentStartDateTime, string serviceName, string barberName, DateTime date, DateTime time, string? notes)
    {
        var appointment = await appointmentRepository.GetByClientEmailAndStartAsync(email, appointmentStartDateTime);
        if (appointment is null)
            return false;

        var selectedService = await serviceCatalogService.ResolveServiceAsync(serviceName);
        var selectedBarber = await barberService.ResolveBarberAsync(barberName);
        var startDateTime = date.Date + time.TimeOfDay;

        appointment.ServiceId = selectedService.ServiceId;
        appointment.BarberId = selectedBarber.BarberId;
        appointment.StartDateTime = startDateTime;
        appointment.EndDateTime = startDateTime.AddMinutes(selectedService.BaseDuration);
        appointment.Notes = notes;
        appointment.Status = "Booked";

        await appointmentRepository.UpdateAsync(appointment);
        return true;
    }

    public async Task<bool> DeleteAsync(string email, DateTime appointmentStartDateTime)
    {
        var appointment = await appointmentRepository.GetByClientEmailAndStartAsync(email, appointmentStartDateTime);
        if (appointment is null)
            return false;

        await appointmentRepository.DeleteAsync(appointment);
        return true;
    }
}
