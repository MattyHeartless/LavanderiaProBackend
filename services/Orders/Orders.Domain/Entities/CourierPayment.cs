namespace Orders.Domain.Entities;

public class CourierPayment
{
    public Guid Id { get; set; }
    public Guid CourierGuid { get; set; }
    public string CourierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int OrdersCount { get; set; }
    public DateTime PaidAt { get; set; }
    public string PaidByAdminId { get; set; } = string.Empty;
    public string? Note { get; set; }
    public List<CourierPaymentOrder> Orders { get; set; } = new();
}
