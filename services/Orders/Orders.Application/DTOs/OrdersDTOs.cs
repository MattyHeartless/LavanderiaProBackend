using Orders.Domain.Entities;

namespace Orders.Application.DTOs;
public class CreateOrderRequest
{
    public Order Order { get; set; } = default!;
    public List<OrderDetail> OrderDetails { get; set; } = default!;

}