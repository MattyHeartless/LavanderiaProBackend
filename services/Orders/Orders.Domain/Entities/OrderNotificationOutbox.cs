namespace Orders.Domain.Entities;

public sealed class OrderNotificationOutbox
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
}
