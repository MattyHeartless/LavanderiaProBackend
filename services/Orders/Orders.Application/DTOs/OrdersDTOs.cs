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

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class CourierWorkedOrdersKpisResponse
{
    public int CompletedOrdersCount { get; set; }
    public decimal TotalEarned { get; set; }
    public List<CompletedOrdersByDayItem> CompletedOrdersByDay { get; set; } = new();
    public List<RecentCompletedOrderItem> RecentCompletedOrders { get; set; } = new();
}

public class CompletedOrdersByDayItem
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public class RecentCompletedOrderItem
{
    public Guid OrderId { get; set; }
    public DateTime CompletedAt { get; set; }
    public decimal DeliveryFee { get; set; }
}