using System.Linq.Expressions;
using BarberShop.Models;
using BarberShop.Repositories.Interfaces;
using BarberShop.Services.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BarberShop.Tests;

internal sealed class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _items = new();

    public InMemoryRepository(IEnumerable<T>? seed = null)
    {
        if (seed is not null)
        {
            _items.AddRange(seed);
        }
    }

    public Task<List<T>> GetAllAsync() => Task.FromResult(_items.ToList());

    public Task<T?> GetByIdAsync(int id)
    {
        var item = _items.FirstOrDefault(entity => GetEntityId(entity) == id);
        return Task.FromResult(item);
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => Task.FromResult(_items.AsQueryable().FirstOrDefault(predicate));

    public Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate)
        => Task.FromResult(_items.AsQueryable().Where(predicate).ToList());

    public Task<T> AddAsync(T entity)
    {
        AssignIdIfNeeded(entity);
        _items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity) => Task.CompletedTask;

    public Task DeleteAsync(T entity)
    {
        var id = GetEntityId(entity);
        if (id.HasValue)
        {
            _items.RemoveAll(item => GetEntityId(item) == id.Value);
        }
        else
        {
            _items.Remove(entity);
        }

        return Task.CompletedTask;
    }

    private static int? GetEntityId(T entity)
    {
        var property = typeof(T).GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(int) && p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

        return property is null ? null : (int?)property.GetValue(entity);
    }

    private static void AssignIdIfNeeded(T entity)
    {
        var property = typeof(T).GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(int) && p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && p.CanWrite);

        if (property is null)
        {
            return;
        }

        var currentValue = (int)(property.GetValue(entity) ?? 0);
        if (currentValue != 0)
        {
            return;
        }

        property.SetValue(entity, 1);
    }
}

internal sealed class InMemoryAppointmentRepository : IAppointmentRepository
{
    private readonly List<Appointment> _appointments = new();

    public InMemoryAppointmentRepository(IEnumerable<Appointment>? seed = null)
    {
        if (seed is not null)
        {
            _appointments.AddRange(seed);
        }
    }

    public Task<List<Appointment>> GetByClientIdWithDetailsAsync(int clientId)
        => Task.FromResult(_appointments.Where(a => a.ClientId == clientId).OrderBy(a => a.StartDateTime).ToList());

    public Task<Appointment?> GetByIdAsync(int appointmentId)
        => Task.FromResult(_appointments.FirstOrDefault(a => a.AppointmentId == appointmentId));

    public Task<Appointment?> GetByIdForClientEmailAsync(int appointmentId, string email)
        => Task.FromResult(_appointments.FirstOrDefault(a => a.AppointmentId == appointmentId && a.Client?.Email == email));

    public Task<Appointment?> GetByClientEmailAndStartAsync(string email, DateTime startDateTime)
        => Task.FromResult(_appointments.FirstOrDefault(a => a.Client?.Email == email && a.StartDateTime == startDateTime));

    public Task<Appointment> AddAsync(Appointment appointment)
    {
        if (appointment.AppointmentId == 0)
        {
            appointment.AppointmentId = _appointments.Count == 0 ? 1 : _appointments.Max(a => a.AppointmentId) + 1;
        }

        _appointments.Add(appointment);
        return Task.FromResult(appointment);
    }

    public Task UpdateAsync(Appointment appointment) => Task.CompletedTask;

    public Task DeleteAsync(Appointment appointment)
    {
        _appointments.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
        return Task.CompletedTask;
    }

    public Task<bool> HasOverlappingAppointmentAsync(int barberId, DateTime start, DateTime end)
        => Task.FromResult(_appointments.Any(a => a.BarberId == barberId && a.Status != "Cancelled" && a.StartDateTime < end && a.EndDateTime > start));

    public Task<bool> HasOverlappingAppointmentAsync(int barberId, DateTime start, DateTime end, int? excludeAppointmentId)
        => Task.FromResult(_appointments.Any(a => a.BarberId == barberId && (!excludeAppointmentId.HasValue || a.AppointmentId != excludeAppointmentId.Value) && a.Status != "Cancelled" && a.StartDateTime < end && a.EndDateTime > start));

    public Task<List<Appointment>> GetUpcomingAsync(DateTime from, DateTime to)
        => Task.FromResult(_appointments.Where(a => a.StartDateTime >= from && a.StartDateTime <= to && a.Status != "Cancelled").OrderBy(a => a.StartDateTime).ToList());
}

internal sealed class TestEmailService : IEmailService
{
    public List<EmailMessage> SentMessages { get; } = new();

    public Task<EmailSendResult> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        SentMessages.Add(message);
        return Task.FromResult(EmailSendResult.Ok());
    }

    public Task<EmailSendResult> SendWelcomeEmailAsync(string email, string firstName, string confirmationLink, CancellationToken cancellationToken = default)
        => Task.FromResult(EmailSendResult.Ok());

    public Task<EmailSendResult> SendPasswordResetEmailAsync(string email, string resetLink, string userName, CancellationToken cancellationToken = default)
        => Task.FromResult(EmailSendResult.Ok());

    public Task<EmailSendResult> SendAppointmentConfirmationAsync(string email, string appointmentDetails, CancellationToken cancellationToken = default)
    {
        SentMessages.Add(new EmailMessage { To = email, Subject = "Appointment confirmation", Body = appointmentDetails });
        return Task.FromResult(EmailSendResult.Ok());
    }

    public Task<EmailSendResult> SendAppointmentReminderAsync(string email, string appointmentDetails, CancellationToken cancellationToken = default)
        => Task.FromResult(EmailSendResult.Ok());

    public Task<EmailSendResult> SendUsingTemplateAsync(string to, string subject, string templateName, Dictionary<string, object> templateData, CancellationToken cancellationToken = default)
        => Task.FromResult(EmailSendResult.Ok());
}

internal sealed class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();

    public IEnumerable<string> Keys => _store.Keys;
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public bool IsAvailable => true;

    public void Clear() => _store.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _store.Remove(key);

    public void Set(string key, byte[] value) => _store[key] = value;

    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
}

internal sealed class TestUserStore : IUserStore<ApplicationUser>
{
    private readonly Dictionary<string, ApplicationUser> _users;

    public TestUserStore(IEnumerable<ApplicationUser> users)
    {
        _users = users.ToDictionary(user => user.Id);
    }

    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            user.Id = Guid.NewGuid().ToString("N");
        }

        _users[user.Id] = user;
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _users.Remove(user.Id);
        return Task.FromResult(IdentityResult.Success);
    }

    public void Dispose()
    {
    }

    public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        => Task.FromResult(_users.TryGetValue(userId, out var user) ? user : null);

    public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        => Task.FromResult(_users.Values.FirstOrDefault(user => string.Equals(user.UserName?.ToUpperInvariant(), normalizedUserName, StringComparison.Ordinal)));

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.UserName?.ToUpperInvariant());

    public Task<string?> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult<string?>(user.Id);

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.UserName);

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _users[user.Id] = user;
        return Task.FromResult(IdentityResult.Success);
    }
}

internal sealed class TestTempDataProvider : Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
    }
}

internal static class TestUserManagerFactory
{
    public static UserManager<ApplicationUser> Create(params ApplicationUser[] users)
    {
        var store = new TestUserStore(users);
        var options = Options.Create(new IdentityOptions());
        return new UserManager<ApplicationUser>(
            store,
            options,
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new LoggerFactory().CreateLogger<UserManager<ApplicationUser>>());
    }
}
