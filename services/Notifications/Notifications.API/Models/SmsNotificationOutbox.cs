namespace Notifications.API.Models;

public sealed class SmsNotificationOutbox
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? CourierId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime NextAttemptAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
