using BarberShop.Data;
using BarberShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BarberShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BarberShopDbContext _context;

        public HomeController(ILogger<HomeController> logger, BarberShopDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Barbers()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Booking()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Booking(string fullName, string email, string service, string barber, DateTime date, DateTime time, string? notes)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(service))
            {
                TempData["BookingError"] = "Please complete all required fields.";
                return RedirectToAction(nameof(Booking));
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);
            if (client is null)
            {
                client = new Client
                {
                    FullName = fullName.Trim(),
                    Email = email.Trim(),
                    Phone = string.Empty,
                    MemberSince = DateTime.UtcNow.Date
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            var selectedService = await ResolveServiceAsync(service);
            var selectedBarber = await ResolveBarberAsync(barber);

            var startDateTime = date.Date + time.TimeOfDay;
            var appointment = new Appointment
            {
                ClientId = client.ClientId,
                BarberId = selectedBarber.BarberId,
                ServiceId = selectedService.ServiceId,
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddMinutes(selectedService.BaseDuration),
                Status = "Booked",
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["BookingSuccess"] = "Your appointment was saved.";
            return RedirectToAction(nameof(Booking));
        }

        public IActionResult Products()
        {
            return View();
        }

        public IActionResult Prices()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Gallery()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string email = "admin@yahoo.com", int? selectedAppointmentId = null)
        {
            var normalizedEmail = (email ?? string.Empty).Trim();
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == normalizedEmail);

            var allAppointments = client is null
                ? new List<Appointment>()
                : await _context.Appointments
                    .Include(a => a.Service)
                    .Include(a => a.Barber)
                    .Where(a => a.ClientId == client.ClientId)
                    .OrderBy(a => a.StartDateTime)
                    .ToListAsync();

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

            var model = new ProfileViewModel
            {
                FullName = client?.FullName ?? "Admin",
                Email = client?.Email ?? normalizedEmail,
                Phone = string.IsNullOrWhiteSpace(client?.Phone) ? "+407--------" : client.Phone,
                MemberSince = client?.MemberSince,
                UpcomingAppointments = upcomingAppointments,
                PastAppointments = pastAppointments,
                SelectedAppointment = selectedAppointment
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointment(int appointmentId, string email, string service, string barber, DateTime date, DateTime time, string? notes)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.Client.Email == email);

            if (appointment is null)
            {
                TempData["BookingError"] = "Appointment not found.";
                return RedirectToAction(nameof(Profile), new { email });
            }

            var selectedService = await ResolveServiceAsync(service);
            var selectedBarber = await ResolveBarberAsync(barber);
            var startDateTime = date.Date + time.TimeOfDay;

            appointment.ServiceId = selectedService.ServiceId;
            appointment.BarberId = selectedBarber.BarberId;
            appointment.StartDateTime = startDateTime;
            appointment.EndDateTime = startDateTime.AddMinutes(selectedService.BaseDuration);
            appointment.Notes = notes;
            appointment.Status = "Booked";

            await _context.SaveChangesAsync();

            TempData["BookingSuccess"] = "Appointment updated.";
            return RedirectToAction(nameof(Profile), new { email, selectedAppointmentId = appointmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment(int appointmentId, string email)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.Client.Email == email);

            if (appointment is null)
            {
                TempData["BookingError"] = "Appointment not found.";
                return RedirectToAction(nameof(Profile), new { email });
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            TempData["BookingSuccess"] = "Appointment deleted.";
            return RedirectToAction(nameof(Profile), new { email });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<Service> ResolveServiceAsync(string service)
        {
            var selectedService = await _context.Services.FirstOrDefaultAsync(s => s.Name == service);
            if (selectedService is not null)
            {
                return selectedService;
            }

            selectedService = new Service
            {
                Name = service,
                BaseDuration = 30,
                BasePrice = 0,
                IsActive = true
            };

            _context.Services.Add(selectedService);
            await _context.SaveChangesAsync();
            return selectedService;
        }

        private async Task<Barber> ResolveBarberAsync(string barber)
        {
            Barber? selectedBarber;
            if (string.Equals(barber, "Any available", StringComparison.OrdinalIgnoreCase))
            {
                selectedBarber = await _context.Barbers.FirstOrDefaultAsync(b => b.IsActive);
            }
            else
            {
                selectedBarber = await _context.Barbers.FirstOrDefaultAsync(b => b.FirstName == barber && b.IsActive);
            }

            if (selectedBarber is not null)
            {
                return selectedBarber;
            }

            selectedBarber = new Barber
            {
                FirstName = string.Equals(barber, "Any available", StringComparison.OrdinalIgnoreCase) ? "Any" : barber,
                LastName = "Available",
                Phone = string.Empty,
                Email = string.Empty,
                HireDate = DateTime.UtcNow.Date,
                IsActive = true
            };

            _context.Barbers.Add(selectedBarber);
            await _context.SaveChangesAsync();
            return selectedBarber;
        }
    }
}
