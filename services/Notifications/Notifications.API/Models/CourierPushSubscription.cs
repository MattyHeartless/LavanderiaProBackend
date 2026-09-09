namespace Notifications.API.Models;

public sealed class CourierPushSubscription
{
    public Guid Id { get; set; }
    public string AuthUserId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string? ContentEncoding { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime LastSeenAt { get; set; }
}
