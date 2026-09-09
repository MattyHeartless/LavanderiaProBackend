namespace Notifications.API.Models;

public sealed class PushSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public DateTimeOffset? ExpirationTime { get; set; }
    public PushSubscriptionKeys Keys { get; set; } = new();
}

public sealed class PushSubscriptionKeys
{
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public sealed class AvailabilityRequest { public bool IsAvailable { get; set; } }
public sealed class NewUnassignedOrderNotification { public Guid OrderId { get; set; } }
