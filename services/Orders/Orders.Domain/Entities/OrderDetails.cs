namespace Orders.Domain.Entities;

public class OrderDetail
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string ServiceId { get; set; } = default!;
    public string ServiceName { get; set; } = default!;

    public int Quantity { get; set; }
    public decimal ServicePrice { get; set; }
    public string UoM { get; set; } = default!;

    // Clothing breakdown (only for Bulto options)
    public int? ColoredClothQuantity { get; set; }
    public int? BlackClothQuantity { get; set; }

    // SubTotal: if clothing breakdown is provided, sum colors; otherwise use Quantity
    public decimal SubTotal
    {
        get
        {
            int effectiveQuantity = (ColoredClothQuantity.GetValueOrDefault() + BlackClothQuantity.GetValueOrDefault()) > 0
                ? ColoredClothQuantity.GetValueOrDefault() + BlackClothQuantity.GetValueOrDefault()
                : Quantity;
            return effectiveQuantity * ServicePrice;
        }
    }

    public Guid?   ServicePricingOptionId { get; set; }
    public string? PricingOptionName      { get; set; }
}
