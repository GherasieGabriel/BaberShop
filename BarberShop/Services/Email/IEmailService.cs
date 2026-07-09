namespace BarberShop.Services.Email;

/// <summary>
/// Service interface for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email with the specified message
    /// </summary>
    Task<EmailSendResult> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome email to a new user
    /// </summary>
    Task<EmailSendResult> SendWelcomeEmailAsync(string email, string firstName, string confirmationLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email
    /// </summary>
    Task<EmailSendResult> SendPasswordResetEmailAsync(string email, string resetLink, string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an appointment confirmation email
    /// </summary>
    Task<EmailSendResult> SendAppointmentConfirmationAsync(string email, string appointmentDetails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an appointment reminder email
    /// </summary>
    Task<EmailSendResult> SendAppointmentReminderAsync(string email, string appointmentDetails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a generic email using a template
    /// </summary>
    Task<EmailSendResult> SendUsingTemplateAsync(string to, string subject, string templateName, Dictionary<string, object> templateData, CancellationToken cancellationToken = default);
}
