using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BarberShop.Services.Email;

/// <summary>
/// Email service implementation using MailKit and SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger, IServiceProvider serviceProvider)
    {
        _settings = settings.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;

        ValidateSettings();
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrEmpty(_settings.SmtpServer))
            _logger.LogWarning("SMTP Server is not configured");
        if (string.IsNullOrEmpty(_settings.FromEmail))
            _logger.LogWarning("From Email is not configured");
    }

    public async Task<EmailSendResult> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.SmtpServer))
            {
                _logger.LogWarning("Email sending skipped: SMTP Server not configured. To: {To}, Subject: {Subject}", 
                    message.To, message.Subject);
                return EmailSendResult.Ok("Email skipped - SMTP not configured");
            }

            var mimeMessage = BuildMimeMessage(message);

            using var client = new SmtpClient();

            // Apply timeout if configured (milliseconds)
            try
            {
                if (_settings.TimeoutSeconds > 0)
                {
                    client.Timeout = _settings.TimeoutSeconds * 1000;
                }
            }
            catch { }

            // Choose secure socket options explicitly to avoid ambiguous behavior
            SecureSocketOptions socketOptions;
            if (_settings.SmtpPort == 465)
            {
                socketOptions = SecureSocketOptions.SslOnConnect;
            }
            else if (_settings.SmtpPort == 587)
            {
                socketOptions = SecureSocketOptions.StartTls;
            }
            else
            {
                socketOptions = _settings.UseSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            }

            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, socketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(_settings.SmtpUsername))
            {
                try
                {
                    // Prevent MailKit from attempting XOAUTH2 when not configured
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                }
                catch { }

                await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, cancellationToken);
            }

            await client.SendAsync(mimeMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}. Subject: {Subject}", message.To, message.Subject);
            return EmailSendResult.Ok($"Email sent successfully to {message.To}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}. Subject: {Subject}", message.To, message.Subject);
            return EmailSendResult.Failed($"Failed to send email: {ex.Message}", ex);
        }
    }

    public async Task<EmailSendResult> SendWelcomeEmailAsync(string email, string firstName, string confirmationLink, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Welcome to BarberShop - Verify Your Email";
            var body = BuildWelcomeEmailBody(firstName, confirmationLink);

            var message = new EmailMessage
            {
                To = email,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            return EmailSendResult.Failed($"Failed to send welcome email: {ex.Message}", ex);
        }
    }

    public async Task<EmailSendResult> SendPasswordResetEmailAsync(string email, string resetLink, string userName, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "BarberShop - Password Reset Request";
            var body = BuildPasswordResetEmailBody(userName, resetLink);

            var message = new EmailMessage
            {
                To = email,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            return EmailSendResult.Failed($"Failed to send password reset email: {ex.Message}", ex);
        }
    }

    public async Task<EmailSendResult> SendAppointmentConfirmationAsync(string email, string appointmentDetails, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "BarberShop - Appointment Confirmed";
            var body = BuildAppointmentConfirmationBody(appointmentDetails);

            var message = new EmailMessage
            {
                To = email,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending appointment confirmation to {Email}", email);
            return EmailSendResult.Failed($"Failed to send appointment confirmation: {ex.Message}", ex);
        }
    }

    public async Task<EmailSendResult> SendAppointmentReminderAsync(string email, string appointmentDetails, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "BarberShop - Appointment Reminder";
            var body = BuildAppointmentReminderBody(appointmentDetails);

            var message = new EmailMessage
            {
                To = email,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending appointment reminder to {Email}", email);
            return EmailSendResult.Failed($"Failed to send appointment reminder: {ex.Message}", ex);
        }
    }

    public async Task<EmailSendResult> SendUsingTemplateAsync(string to, string subject, string templateName, Dictionary<string, object> templateData, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement template rendering in later phase
            _logger.LogWarning("Template rendering not yet implemented for template: {TemplateName}", templateName);

            var message = new EmailMessage
            {
                To = to,
                Subject = subject,
                Body = "Template rendering coming soon",
                IsHtml = true
            };

            return await SendEmailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending templated email to {To}", to);
            return EmailSendResult.Failed($"Failed to send templated email: {ex.Message}", ex);
        }
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mimeMessage = new MimeMessage();

        if (!string.IsNullOrEmpty(_settings.FromName))
        {
            mimeMessage.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        }
        else
        {
            mimeMessage.From.Add(new MailboxAddress("BarberShop", _settings.FromEmail));
        }

        mimeMessage.To.Add(new MailboxAddress(message.To, message.To));
        mimeMessage.Subject = message.Subject;

        // Add CC addresses
        foreach (var cc in message.CcAddresses)
        {
            mimeMessage.Cc.Add(new MailboxAddress(cc, cc));
        }

        // Add BCC addresses
        foreach (var bcc in message.BccAddresses)
        {
            mimeMessage.Bcc.Add(new MailboxAddress(bcc, bcc));
        }

        // Set the body
        var bodyBuilder = new BodyBuilder();
        if (message.IsHtml)
        {
            bodyBuilder.HtmlBody = message.Body;
        }
        else
        {
            bodyBuilder.TextBody = message.Body;
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        return mimeMessage;
    }

    private string BuildWelcomeEmailBody(string firstName, string confirmationLink)
    {
        return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #1a1a1a; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f5f5f5; }}
                    .btn {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Welcome to BarberShop!</h1>
                    </div>
                    <div class='content'>
                        <p>Hello {firstName},</p>
                        <p>Thank you for registering with BarberShop. We're excited to have you on board!</p>
                        <p>To complete your registration, please verify your email by clicking the button below:</p>
                        <a href='{confirmationLink}' class='btn'>Verify Email Address</a>
                        <p>If the button doesn't work, you can also copy and paste this link into your browser:</p>
                        <p><small>{confirmationLink}</small></p>
                        <p>This link will expire in 24 hours for security reasons.</p>
                        <p>Best regards,<br/>The BarberShop Team</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2025 BarberShop. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private string BuildPasswordResetEmailBody(string userName, string resetLink)
    {
        return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #1a1a1a; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f5f5f5; }}
                    .btn {{ display: inline-block; padding: 10px 20px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    .warning {{ background-color: #fff3cd; padding: 10px; border-left: 4px solid #ffc107; margin: 15px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Password Reset Request</h1>
                    </div>
                    <div class='content'>
                        <p>Hello {userName},</p>
                        <p>We received a request to reset your password. If you didn't make this request, you can safely ignore this email.</p>
                        <p>To reset your password, click the button below:</p>
                        <a href='{resetLink}' class='btn'>Reset Password</a>
                        <p>If the button doesn't work, you can also copy and paste this link into your browser:</p>
                        <p><small>{resetLink}</small></p>
                        <div class='warning'>
                            <strong>Security Notice:</strong> This link will expire in 1 hour for security reasons. Never share this link with anyone.
                        </div>
                        <p>Best regards,<br/>The BarberShop Team</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2025 BarberShop. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private string BuildAppointmentConfirmationBody(string appointmentDetails)
    {
        return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #1a1a1a; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f5f5f5; }}
                    .details {{ background-color: white; padding: 15px; border-left: 4px solid #28a745; margin: 15px 0; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Appointment Confirmed</h1>
                    </div>
                    <div class='content'>
                        <p>Your appointment has been successfully confirmed!</p>
                        <div class='details'>
                            {appointmentDetails}
                        </div>
                        <p>Please arrive 5-10 minutes early. If you need to reschedule or cancel, please do so at least 24 hours in advance.</p>
                        <p>Best regards,<br/>The BarberShop Team</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2025 BarberShop. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private string BuildAppointmentReminderBody(string appointmentDetails)
    {
        return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #1a1a1a; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f5f5f5; }}
                    .details {{ background-color: white; padding: 15px; border-left: 4px solid #ffc107; margin: 15px 0; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Appointment Reminder</h1>
                    </div>
                    <div class='content'>
                        <p>This is a friendly reminder about your upcoming appointment:</p>
                        <div class='details'>
                            {appointmentDetails}
                        </div>
                        <p>Please arrive on time. We look forward to seeing you!</p>
                        <p>Best regards,<br/>The BarberShop Team</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2025 BarberShop. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
