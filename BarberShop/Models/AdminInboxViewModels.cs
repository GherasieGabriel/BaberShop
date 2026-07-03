namespace BarberShop.Models;

public class AdminInboxMessageItemViewModel
{
    public int MessageId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Preview => Message.Length <= 120 ? Message : Message[..120] + "...";
}

public class AdminInboxViewModel
{
    public List<AdminInboxMessageItemViewModel> Messages { get; set; } = new();
    public AdminInboxMessageItemViewModel? SelectedMessage { get; set; }
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public bool UnreadOnly { get; set; }
    public int? SelectedMessageId { get; set; }
}
