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

public class CourierPaymentSummaryItem
{
    public Guid CourierGuid { get; set; }
    public string CourierName { get; set; } = string.Empty;
    public int PendingOrdersCount { get; set; }
    public decimal PendingAmount { get; set; }
    public DateTime? LastPaidAt { get; set; }
    public decimal? LastPaidAmount { get; set; }
}

public class CourierPaymentDetailResponse : CourierPaymentSummaryItem
{
    public List<CourierPaymentPendingOrderItem> PendingOrders { get; set; } = new();
    public List<CourierPaymentHistoryItem> Payments { get; set; } = new();
}

public class CourierPaymentPendingOrderItem
{
    public Guid OrderId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime CompletedAt { get; set; }
    public decimal Amount { get; set; }
}

public class CourierPaymentHistoryItem
{
    public Guid PaymentId { get; set; }
    public decimal TotalAmount { get; set; }
    public int OrdersCount { get; set; }
    public DateTime PaidAt { get; set; }
    public string? Note { get; set; }
    public List<CourierPaymentPaidOrderItem> Orders { get; set; } = new();
}

public class CourierPaymentPaidOrderItem
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}

public class RegisterCourierPaymentRequest
{
    public string? Note { get; set; }
}

public class RegisterCourierPaymentResponse
{
    public Guid PaymentId { get; set; }
    public decimal TotalAmount { get; set; }
    public int OrdersCount { get; set; }
    public DateTime PaidAt { get; set; }
}
