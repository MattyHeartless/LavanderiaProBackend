namespace Orders.Domain.Entities;

public sealed class ClientOrderSmsNotificationOutbox
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public ClientOrderSmsNotificationEventType EventType { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
}
