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

            var selectedService = await _context.Services.FirstOrDefaultAsync(s => s.Name == service);
            if (selectedService is null)
            {
                selectedService = new Service
                {
                    Name = service,
                    BaseDuration = 30,
                    BasePrice = 0,
                    IsActive = true
                };
                _context.Services.Add(selectedService);
                await _context.SaveChangesAsync();
            }

            Barber? selectedBarber;
            if (string.Equals(barber, "Any available", StringComparison.OrdinalIgnoreCase))
            {
                selectedBarber = await _context.Barbers.FirstOrDefaultAsync(b => b.IsActive);
            }
            else
            {
                selectedBarber = await _context.Barbers.FirstOrDefaultAsync(b => b.FirstName == barber && b.IsActive);
            }

            if (selectedBarber is null)
            {
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
            }

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

        public IActionResult Profile()
        {
            return View();
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
    }
}
