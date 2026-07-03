namespace BarberShop.Services.Email;

/// <summary>
/// Represents an email message
/// </summary>
public class EmailMessage
{
    public string? To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public string? From { get; set; }
    public List<string> CcAddresses { get; set; } = new();
    public List<string> BccAddresses { get; set; } = new();
    public Dictionary<string, string> Attachments { get; set; } = new();
}

/// <summary>
/// Represents email settings for SMTP configuration
/// </summary>
public class EmailSettings
{
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? SendGridApiKey { get; set; }
    public bool UseSSL { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 10;
}

/// <summary>
/// Result of an email send operation
/// </summary>
public class EmailSendResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }

    public static EmailSendResult Ok(string message = "Email sent successfully")
    {
        return new EmailSendResult { Success = true, Message = message };
    }

    public static EmailSendResult Failed(string message, Exception? ex = null)
    {
        return new EmailSendResult { Success = false, Message = message, Exception = ex };
    }
}

/// <summary>
/// Email template parameters for rendering
/// </summary>
public class EmailTemplateModel
{
    public string? TemplateName { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}
