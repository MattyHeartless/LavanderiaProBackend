namespace Orders.Domain.Entities;

public class CourierPaymentOrder
{
    public Guid Id { get; set; }
    public Guid CourierPaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal PaidAmount { get; set; }

    public CourierPayment CourierPayment { get; set; } = default!;
}
