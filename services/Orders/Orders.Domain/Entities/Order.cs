namespace Orders.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public int UserAddressId { get; set; }
    public ShippingAddress ShippingAddress { get; set; } = default!;
    public int? UserPaymentMethodId { get; set; }

    public DateOnly PickupDate { get; set; }
    public TimeOnly PickupTime { get; set; }

    public Boolean IsPostPayment { get; set; }
    public string PostPaymentMethod { get; set; } = default!;

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public int? CourierId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RecollectedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

}
