using Orders.Domain.Entities;

namespace Orders.Application.DTOs;
public class CreateOrderRequest
{
    public Order Order { get; set; } = default!;
    public List<OrderDetail> OrderDetails { get; set; } = default!;

}


public class RetrieveOrders
{
    public Order Order { get; set; } = default!;
    
    public List<OrderDetail> OrderDetails { get; set; } = new();
}

public class AssignOrderCourierRequest
{
    public Guid CourierGuid { get; set; }
    public string CourierName { get; set; } = default!;
    public string? CourierPhone { get; set; }
}