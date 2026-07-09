using BarberShop.Controllers;
using BarberShop.Models;
using BarberShop.Services;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BarberShop.Tests;

public class AppointmentAndPaymentTests
{
    [Fact]
    public async Task AppointmentService_CreateAsync_BooksAppointment()
    {
        var clientRepo = new InMemoryRepository<Client>();
        var appointmentRepo = new InMemoryAppointmentRepository();
        var serviceRepo = new InMemoryRepository<Service>(new[]
        {
            new Service { ServiceId = 1, Name = "Haircut", BaseDuration = 30, BasePrice = 50m, IsActive = true }
        });
        var barberRepo = new InMemoryRepository<Barber>(new[]
        {
            new Barber { BarberId = 1, FirstName = "Alex", LastName = "Pop", Email = "alex@shop.com", Phone = "123", HireDate = DateTime.UtcNow.Date, IsActive = true }
        });
        var service = new AppointmentService(
            appointmentRepo,
            new ClientService(clientRepo),
            clientRepo,
            new BarberService(barberRepo),
            new ServiceCatalogService(serviceRepo),
            new TestEmailService());

        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var created = await service.CreateAsync(new BookingViewModel
        {
            FullName = "Jane Doe",
            Email = "jane@shop.com",
            Service = "Haircut",
            Barber = "Alex Pop",
            Date = startDate,
            Time = new TimeSpan(14, 0, 0),
            Notes = "First visit"
        });

        var expectedStart = DateTime.SpecifyKind(startDate + new TimeSpan(14, 0, 0), DateTimeKind.Local).ToUniversalTime();
        var appointment = await appointmentRepo.GetByIdAsync(1);

        Assert.True(created);
        Assert.NotNull(appointment);
        Assert.Equal("Booked", appointment!.Status);
        Assert.Equal("First visit", appointment.Notes);
        Assert.Equal(expectedStart, appointment.StartDateTime);
        Assert.Equal(expectedStart.AddMinutes(30), appointment.EndDateTime);
    }

    [Fact]
    public async Task AppointmentService_CreateAsync_RejectsConflictingAppointment()
    {
        var client = new Client { ClientId = 1, FullName = "Jane Doe", Email = "jane@shop.com", Phone = "", MemberSince = DateTime.UtcNow.Date };
        var existingStart = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(1).AddHours(14), DateTimeKind.Local).ToUniversalTime();
        var appointmentRepo = new InMemoryAppointmentRepository(new[]
        {
            new Appointment
            {
                AppointmentId = 1,
                ClientId = client.ClientId,
                BarberId = 1,
                ServiceId = 1,
                StartDateTime = existingStart,
                EndDateTime = existingStart.AddMinutes(30),
                Status = "Booked",
                CreatedAt = DateTime.UtcNow,
                Client = client,
                Barber = new Barber { BarberId = 1, FirstName = "Alex", LastName = "Pop", Email = "alex@shop.com", IsActive = true },
                Service = new Service { ServiceId = 1, Name = "Haircut", BaseDuration = 30, BasePrice = 50m, IsActive = true }
            }
        });
        var clientRepo = new InMemoryRepository<Client>(new[] { client });
        var serviceRepo = new InMemoryRepository<Service>(new[]
        {
            new Service { ServiceId = 1, Name = "Haircut", BaseDuration = 30, BasePrice = 50m, IsActive = true }
        });
        var barberRepo = new InMemoryRepository<Barber>(new[]
        {
            new Barber { BarberId = 1, FirstName = "Alex", LastName = "Pop", Email = "alex@shop.com", Phone = "123", HireDate = DateTime.UtcNow.Date, IsActive = true }
        });
        var service = new AppointmentService(
            appointmentRepo,
            new ClientService(clientRepo),
            clientRepo,
            new BarberService(barberRepo),
            new ServiceCatalogService(serviceRepo),
            new TestEmailService());

        var created = await service.CreateAsync(new BookingViewModel
        {
            FullName = client.FullName,
            Email = client.Email,
            Service = "Haircut",
            Barber = "Alex Pop",
            Date = DateTime.UtcNow.Date.AddDays(1),
            Time = new TimeSpan(14, 15, 0)
        });

        Assert.False(created);
        Assert.Single(await appointmentRepo.GetUpcomingAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(2)));
    }

    [Fact]
    public async Task AppointmentService_GetProfileAsync_SplitsUpcomingAndPastAppointments()
    {
        var client = new Client { ClientId = 7, FullName = "Jane Doe", Email = "jane@shop.com", Phone = "", MemberSince = DateTime.UtcNow.Date };
        var now = DateTime.UtcNow;
        var upcoming = new Appointment
        {
            AppointmentId = 1,
            ClientId = client.ClientId,
            BarberId = 1,
            ServiceId = 1,
            StartDateTime = now.AddDays(1),
            EndDateTime = now.AddDays(1).AddMinutes(30),
            Status = "Booked",
            CreatedAt = now,
            Client = client,
            Barber = new Barber { BarberId = 1, FirstName = "Alex", LastName = "Pop", Email = "alex@shop.com", IsActive = true },
            Service = new Service { ServiceId = 1, Name = "Haircut", BaseDuration = 30, BasePrice = 50m, IsActive = true }
        };
        var past = new Appointment
        {
            AppointmentId = 2,
            ClientId = client.ClientId,
            BarberId = 1,
            ServiceId = 1,
            StartDateTime = now.AddDays(-1),
            EndDateTime = now.AddDays(-1).AddMinutes(30),
            Status = "Booked",
            CreatedAt = now,
            Client = client,
            Barber = upcoming.Barber,
            Service = upcoming.Service
        };

        var appointmentRepo = new InMemoryAppointmentRepository(new[] { upcoming, past });
        var clientRepo = new InMemoryRepository<Client>(new[] { client });
        var service = new AppointmentService(
            appointmentRepo,
            new ClientService(clientRepo),
            clientRepo,
            new BarberService(new InMemoryRepository<Barber>(new[] { upcoming.Barber })),
            new ServiceCatalogService(new InMemoryRepository<Service>(new[] { upcoming.Service })),
            new TestEmailService());

        var model = await service.GetProfileAsync(client.Email);

        Assert.Single(model.UpcomingAppointments);
        Assert.Single(model.PastAppointments);
        Assert.Equal(upcoming.AppointmentId, model.SelectedAppointment?.AppointmentId);
    }

    [Fact]
    public async Task AppointmentService_UpdateAndDelete_Works()
    {
        var client = new Client { ClientId = 1, FullName = "Jane Doe", Email = "jane@shop.com", Phone = "", MemberSince = DateTime.UtcNow.Date };
        var appointment = new Appointment
        {
            AppointmentId = 1,
            ClientId = client.ClientId,
            BarberId = 1,
            ServiceId = 1,
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            Status = "Booked",
            CreatedAt = DateTime.UtcNow,
            Client = client,
            Barber = new Barber { BarberId = 1, FirstName = "Alex", LastName = "Pop", Email = "alex@shop.com", IsActive = true },
            Service = new Service { ServiceId = 1, Name = "Haircut", BaseDuration = 30, BasePrice = 50m, IsActive = true }
        };
        var appointmentRepo = new InMemoryAppointmentRepository(new[] { appointment });
        var clientRepo = new InMemoryRepository<Client>(new[] { client });
        var barberRepo = new InMemoryRepository<Barber>(new[]
        {
            new Barber { BarberId = 2, FirstName = "Mark", LastName = "Stone", Email = "mark@shop.com", Phone = "444", HireDate = DateTime.UtcNow.Date, IsActive = true }
        });
        var serviceRepo = new InMemoryRepository<Service>(new[]
        {
            new Service { ServiceId = 2, Name = "Beard Trim", BaseDuration = 20, BasePrice = 30m, IsActive = true }
        });
        var service = new AppointmentService(
            appointmentRepo,
            new ClientService(clientRepo),
            clientRepo,
            new BarberService(barberRepo),
            new ServiceCatalogService(serviceRepo),
            new TestEmailService());

        var updated = await service.UpdateAsync(1, "Beard Trim", "Mark Stone", DateTime.UtcNow.Date.AddDays(2), DateTime.UtcNow.Date.AddHours(11), "Updated note");
        var deleted = await service.DeleteAsync(1);

        Assert.True(updated);
        Assert.True(deleted);
        Assert.Empty(await appointmentRepo.GetUpcomingAsync(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(5)));
    }

    [Fact]
    public async Task PaymentsController_Create_CreatesPaymentAndMarksAppointmentPaid()
    {
        var client = new Client { ClientId = 1, FullName = "Jane Doe", Email = "jane@shop.com", Phone = "", MemberSince = DateTime.UtcNow.Date };
        var appointment = new Appointment
        {
            AppointmentId = 1,
            ClientId = client.ClientId,
            BarberId = 1,
            ServiceId = 1,
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            Status = "Booked",
            CreatedAt = DateTime.UtcNow,
            Client = client,
            Barber = new Barber { BarberId = 1, FirstName = "Alex", LastName = "Pop", Email = "alex@shop.com", IsActive = true },
            Service = new Service { ServiceId = 1, Name = "Haircut", BaseDuration = 30, BasePrice = 50m, IsActive = true }
        };
        var appointmentRepo = new InMemoryAppointmentRepository(new[] { appointment });
        var paymentRepo = new InMemoryRepository<Payment>();
        var emailService = new TestEmailService();
        var controller = new PaymentsController(paymentRepo, appointmentRepo, emailService)
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(new Microsoft.AspNetCore.Http.DefaultHttpContext(), new TestTempDataProvider())
        };

        var result = await controller.Create(1, 50m);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        var savedPayment = await paymentRepo.FirstOrDefaultAsync(p => p.AppointmentId == 1);

        Assert.Equal("Profile", redirect.ActionName);
        Assert.NotNull(savedPayment);
        Assert.Equal("Paid", appointment.Status);
        Assert.Equal(1, emailService.SentMessages.Count);
    }

    [Fact]
    public async Task PaymentsController_Refund_UpdatesPaymentAndAppointment()
    {
        var client = new Client { ClientId = 1, FullName = "Jane Doe", Email = "jane@shop.com", Phone = "", MemberSince = DateTime.UtcNow.Date };
        var appointment = new Appointment
        {
            AppointmentId = 1,
            ClientId = client.ClientId,
            BarberId = 1,
            ServiceId = 1,
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            Status = "Paid",
            CreatedAt = DateTime.UtcNow,
            Client = client,
            Barber = new Barber { BarberId = 1, FirstName = "Alex", LastName = "Pop", Email = "alex@shop.com", IsActive = true },
            Service = new Service { ServiceId = 1, Name = "Haircut", BaseDuration = 30, BasePrice = 50m, IsActive = true }
        };
        var payment = new Payment
        {
            PaymentId = 1,
            AppointmentId = 1,
            Amount = 50m,
            Currency = "RON",
            Method = "Dummy",
            Status = "Paid",
            PaidAt = DateTime.UtcNow,
            TransactionRef = "DUMMY-123",
            CreatedAt = DateTime.UtcNow,
            Appointment = appointment
        };
        var appointmentRepo = new InMemoryAppointmentRepository(new[] { appointment });
        var paymentRepo = new InMemoryRepository<Payment>(new[] { payment });
        var controller = new PaymentsController(paymentRepo, appointmentRepo, new TestEmailService())
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(new Microsoft.AspNetCore.Http.DefaultHttpContext(), new TestTempDataProvider())
        };

        var result = await controller.Refund(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        var updatedPayment = await paymentRepo.GetByIdAsync(1);
        var updatedAppointment = await appointmentRepo.GetByIdAsync(1);

        Assert.Equal("Profile", redirect.ActionName);
        Assert.Equal("Refunded", updatedPayment!.Status);
        Assert.Equal("Cancelled", updatedAppointment!.Status);
    }
}
