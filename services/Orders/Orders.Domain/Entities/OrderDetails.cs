namespace Orders.Domain.Entities;

public class OrderDetail
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string ServiceId { get; set; } = default!;
    public string ServiceName { get; set; } = default!;

    public int Quantity { get; set; }
    public decimal ServicePrice { get; set; }

    public decimal SubTotal => Quantity * ServicePrice;
}
