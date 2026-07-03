using BarberShop.Repositories.Interfaces;
using BarberShop.Services.Email;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BarberShop.Services;

public class BackgroundReminderService : BackgroundService
{
    private readonly ILogger<BackgroundReminderService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    // In-memory tracking to avoid duplicate reminders in this demo
    private readonly HashSet<int> _sentReminders = new();

    public BackgroundReminderService(ILogger<BackgroundReminderService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackgroundReminderService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWork(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reminder service loop");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        // create a scope so we can resolve scoped services like the repository and email service
        using var scope = _scopeFactory.CreateScope();
        var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Send reminders for appointments starting within next 24 hours
        var now = DateTime.UtcNow;
        var windowEnd = now.AddHours(24);
        var upcoming = await appointmentRepository.GetUpcomingAsync(now, windowEnd);

        foreach (var appt in upcoming)
        {
            if (_sentReminders.Contains(appt.AppointmentId))
                continue;

            var localStart = appt.StartDateTime.ToLocalTime();
            var timeUntil = appt.StartDateTime - now;
            if (timeUntil.TotalSeconds > 0 && timeUntil.TotalHours <= 24)
            {
                try
                {
                    var details = $"Reminder: your appointment for {appt.Service.Name} with {appt.Barber.FirstName} {appt.Barber.LastName} is scheduled at {localStart:f}.";
                    await emailService.SendAppointmentReminderAsync(appt.Client.Email, details);
                    _sentReminders.Add(appt.AppointmentId);
                    _logger.LogInformation("Sent reminder for appointment {Id}", appt.AppointmentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder for appointment {Id}", appt.AppointmentId);
                }
            }
        }
    }
}
