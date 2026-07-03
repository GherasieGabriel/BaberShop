using BarberShop.Models;
using BarberShop.Repositories.Interfaces;
using BarberShop.Services.Interfaces;
using BarberShop.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IEmailService _emailService;

    public PaymentsController(IRepository<Payment> paymentRepository, IAppointmentRepository appointmentRepository, IEmailService emailService)
    {
        _paymentRepository = paymentRepository;
        _appointmentRepository = appointmentRepository;
        _emailService = emailService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Create(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment is null)
        {
            return NotFound();
        }

        return View(new PaymentCheckoutViewModel
        {
            AppointmentId = appointment.AppointmentId,
            Appointment = appointment,
            Amount = appointment.Service.BasePrice,
            Currency = "RON"
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int appointmentId, decimal amount)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment is null)
        {
            return NotFound();
        }

        var payment = new Payment
        {
            AppointmentId = appointmentId,
            Amount = amount,
            Currency = "RON",
            Method = "Dummy",
            Status = "Paid",
            PaidAt = DateTime.UtcNow,
            TransactionRef = "DUMMY-" + Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);

        // Optionally mark appointment as paid
        appointment.Status = "Paid";
        await _appointmentRepository.UpdateAsync(appointment);

        // Send payment confirmation email
        try
        {
            var details = $"Payment received for your appointment (ID: {appointmentId}). Amount: {amount} {payment.Currency}. Transaction: {payment.TransactionRef}.";
            await _emailService.SendAppointmentConfirmationAsync(appointment.Client?.Email ?? string.Empty, details);
        }
        catch { }

        TempData["BookingSuccess"] = "Payment processed (dummy).";
        return RedirectToAction("Profile", "Appointments", new { selectedAppointmentId = appointmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(int paymentId)
    {
        var payment = await _paymentRepository.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        if (payment is null)
        {
            TempData["BookingError"] = "Payment record not found.";
            return RedirectToAction("Index", "Appointments");
        }

        // Simple dummy refund: mark status refunded and create refund transaction id
        payment.Status = "Refunded";
        payment.TransactionRef = (payment.TransactionRef ?? "") + "-REFUND-" + Guid.NewGuid().ToString("N");
        await _paymentRepository.UpdateAsync(payment);

        // Update appointment status if needed
        var appointment = await _appointmentRepository.GetByIdAsync(payment.AppointmentId);
        if (appointment is not null)
        {
            appointment.Status = "Cancelled";
            await _appointmentRepository.UpdateAsync(appointment);
        }

        try
        {
            var details = $"Your payment (ID: {paymentId}) has been refunded. Transaction: {payment.TransactionRef}.";
            await _emailService.SendAppointmentConfirmationAsync(appointment?.Client?.Email ?? string.Empty, details);
        }
        catch { }

        TempData["BookingSuccess"] = "Refund issued (dummy).";
        return RedirectToAction("Profile", "Appointments", new { selectedAppointmentId = payment.AppointmentId });
    }
}
