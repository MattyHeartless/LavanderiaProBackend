using Microsoft.EntityFrameworkCore;
using Orders.Application.DTOs;
using Orders.Application.Repositories;
using Orders.Domain.Entities;

using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<IEnumerable<RetrieveOrders>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new RetrieveOrders
            {
                Order = o,
                OrderDetails = _context.OrderDetails
                    .Where(d => d.OrderId == o.Id)
                    .ToList()
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RetrieveOrders>> GetUnassignedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => !o.CourierGuid.HasValue || o.CourierGuid == Guid.Empty)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new RetrieveOrders
            {
                Order = o,
                OrderDetails = _context.OrderDetails
                    .Where(d => d.OrderId == o.Id)
                    .ToList()
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

      public async Task<List<OrderDetail>> GetOrderDetailsByOrderId(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.OrderDetails
            .Where(o => o.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

   public async Task<IEnumerable<RetrieveOrders>> GetByUserIdAsync(
    string userId, 
    CancellationToken cancellationToken = default)
{
    return await _context.Orders
        .Where(o => o.UserId == userId)
        .OrderByDescending(o => o.CreatedAt)
        .Select(o => new RetrieveOrders
        {
            Order = o,
            // Aquí hacemos la búsqueda manual de los detalles por Id
            OrderDetails = _context.OrderDetails
                .Where(d => d.OrderId == o.Id)
                .ToList()
        })
        .AsNoTracking()
        .ToListAsync(cancellationToken);
}

    public async Task<IEnumerable<RetrieveOrders>> GetByCourierGuidAsync(
        Guid courierGuid,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CourierGuid == courierGuid
                && o.Status >= OrderStatus.Created
                && o.Status <= OrderStatus.Delivering)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new RetrieveOrders
            {
                Order = o,
                OrderDetails = _context.OrderDetails
                    .Where(d => d.OrderId == o.Id)
                    .ToList()
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<CourierWorkedOrdersKpisResponse> GetCourierWorkedOrdersKpisAsync(
        Guid courierGuid,
        CancellationToken cancellationToken = default)
    {
        var completedOrders = await _context.Orders
            .Where(o => o.CourierGuid == courierGuid && o.Status == OrderStatus.Completed)
            .Select(o => new
            {
                o.Id,
                o.DeliveryFee,
                CompletedAt = (o.DeliveredAt ?? o.CreatedAt)
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var groupedCounts = completedOrders
            .GroupBy(x => x.CompletedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var today = DateTime.UtcNow.Date;
        var completedOrdersByDay = Enumerable.Range(-3, 7)
            .Select(offset => today.AddDays(offset))
            .Select(day => new CompletedOrdersByDayItem
            {
                Date = DateOnly.FromDateTime(day),
                Count = groupedCounts.TryGetValue(day, out var count) ? count : 0
            })
            .ToList();

        var recentCompletedOrders = completedOrders
            .OrderByDescending(x => x.CompletedAt)
            .Take(3)
            .Select(x => new RecentCompletedOrderItem
            {
                OrderId = x.Id,
                CompletedAt = x.CompletedAt,
                DeliveryFee = x.DeliveryFee
            })
            .ToList();

        return new CourierWorkedOrdersKpisResponse
        {
            CompletedOrdersCount = completedOrders.Count,
            TotalEarned = completedOrders.Sum(x => x.DeliveryFee),
            CompletedOrdersByDay = completedOrdersByDay,
            RecentCompletedOrders = recentCompletedOrders
        };
    }

    public async Task<List<CourierPaymentSummaryItem>> GetCourierPaymentSummariesAsync(
        CancellationToken cancellationToken = default)
    {
        var pendingOrders = await GetPendingCourierPaymentOrdersQuery()
            .Select(order => new
            {
                CourierGuid = order.CourierGuid!.Value,
                order.CourierName,
                order.DeliveryFee
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var latestPayments = await _context.CourierPayments
            .GroupBy(payment => payment.CourierGuid)
            .Select(group => group.OrderByDescending(payment => payment.PaidAt).First())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return pendingOrders
            .GroupBy(order => order.CourierGuid)
            .Select(group =>
            {
                var latestPayment = latestPayments.FirstOrDefault(payment => payment.CourierGuid == group.Key);
                return new CourierPaymentSummaryItem
                {
                    CourierGuid = group.Key,
                    CourierName = group.Select(order => order.CourierName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty,
                    PendingOrdersCount = group.Count(),
                    PendingAmount = group.Sum(order => order.DeliveryFee),
                    LastPaidAt = latestPayment?.PaidAt,
                    LastPaidAmount = latestPayment?.TotalAmount
                };
            })
            .OrderByDescending(item => item.PendingAmount)
            .ToList();
    }

    public async Task<CourierPaymentDetailResponse> GetCourierPaymentDetailAsync(
        Guid courierGuid,
        CancellationToken cancellationToken = default)
    {
        var pendingOrders = await GetPendingCourierPaymentOrdersQuery()
            .Where(order => order.CourierGuid == courierGuid)
            .OrderBy(order => order.DeliveredAt ?? order.CreatedAt)
            .Select(order => new CourierPaymentPendingOrderItem
            {
                OrderId = order.Id,
                CustomerName = order.UserName,
                CompletedAt = order.DeliveredAt ?? order.CreatedAt,
                Amount = order.DeliveryFee
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var payments = await _context.CourierPayments
            .Where(payment => payment.CourierGuid == courierGuid)
            .Include(payment => payment.Orders)
            .OrderByDescending(payment => payment.PaidAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var courierName = pendingOrders.Count > 0
            ? await _context.Orders
                .Where(order => order.CourierGuid == courierGuid)
                .OrderByDescending(order => order.CreatedAt)
                .Select(order => order.CourierName)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty
            : payments.FirstOrDefault()?.CourierName ?? string.Empty;

        var latestPayment = payments.FirstOrDefault();
        return new CourierPaymentDetailResponse
        {
            CourierGuid = courierGuid,
            CourierName = courierName,
            PendingOrdersCount = pendingOrders.Count,
            PendingAmount = pendingOrders.Sum(order => order.Amount),
            LastPaidAt = latestPayment?.PaidAt,
            LastPaidAmount = latestPayment?.TotalAmount,
            PendingOrders = pendingOrders,
            Payments = payments.Select(payment => new CourierPaymentHistoryItem
            {
                PaymentId = payment.Id,
                TotalAmount = payment.TotalAmount,
                OrdersCount = payment.OrdersCount,
                PaidAt = payment.PaidAt,
                Note = payment.Note,
                Orders = payment.Orders
                    .OrderBy(paymentOrder => paymentOrder.OrderId)
                    .Select(paymentOrder => new CourierPaymentPaidOrderItem
                    {
                        OrderId = paymentOrder.OrderId,
                        Amount = paymentOrder.PaidAmount
                    })
                    .ToList()
            }).ToList()
        };
    }

    public async Task<RegisterCourierPaymentResponse?> RegisterCourierPaymentAsync(
        Guid courierGuid,
        string paidByAdminId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

        var pendingOrders = await GetPendingCourierPaymentOrdersQuery()
            .Where(order => order.CourierGuid == courierGuid)
            .OrderBy(order => order.Id)
            .ToListAsync(cancellationToken);

        if (pendingOrders.Count == 0)
            return null;

        var paidAt = DateTime.UtcNow;
        var payment = new CourierPayment
        {
            Id = Guid.NewGuid(),
            CourierGuid = courierGuid,
            CourierName = pendingOrders.Select(order => order.CourierName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty,
            TotalAmount = pendingOrders.Sum(order => order.DeliveryFee),
            OrdersCount = pendingOrders.Count,
            PaidAt = paidAt,
            PaidByAdminId = paidByAdminId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            Orders = pendingOrders.Select(order => new CourierPaymentOrder
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                PaidAmount = order.DeliveryFee
            }).ToList()
        };

        _context.CourierPayments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new RegisterCourierPaymentResponse
        {
            PaymentId = payment.Id,
            TotalAmount = payment.TotalAmount,
            OrdersCount = payment.OrdersCount,
            PaidAt = payment.PaidAt
        };
    }

    private IQueryable<Order> GetPendingCourierPaymentOrdersQuery() =>
        _context.Orders.Where(order =>
            order.Status == OrderStatus.Completed &&
            order.CourierGuid.HasValue &&
            order.CourierGuid != Guid.Empty &&
            !_context.CourierPaymentOrders.Any(paymentOrder => paymentOrder.OrderId == order.Id));

    public async Task<List<DeliveryMode>> GetActiveDeliveryModesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryModes
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<DeliveryMode?> GetDeliveryModeByIdAsync(int deliveryModeId, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryModes
            .FirstOrDefaultAsync(x => x.Id == deliveryModeId, cancellationToken);
    }

    public async Task<Guid> AddAsync(
        Order order,
        IEnumerable<OrderDetail> orderDetails,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        if (order.Id == Guid.Empty)
            order.Id = Guid.NewGuid();
        await _context.Orders.AddAsync(order, cancellationToken);
        foreach (var detail in orderDetails)
        {
            detail.OrderId = order.Id;
        }
        await _context.OrderDetails.AddRangeAsync(orderDetails, cancellationToken);
        await _context.OrderNotificationOutbox.AddAsync(new OrderNotificationOutbox
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return order.Id;
    }

    public async Task AddDetailAsync(
        OrderDetail detail,
        CancellationToken cancellationToken = default)
    {
        await _context.OrderDetails.AddAsync(detail, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AssignCourierAsync(
        Guid orderId,
        AssignOrderCourierRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return false;

        order.CourierGuid = request.CourierGuid;
        order.CourierName = request.CourierName;
        order.CourierPhone = request.CourierPhone;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return false;

        var currentStatus = (int)order.Status;
        var newStatus = (int)request.Status;

        // Allow step-by-step transitions; special rule allows jumping to Recollecting from Created or Paid.
        var isSequentialTransition = newStatus == currentStatus + 1;
        var isValidRecollectingTransition = request.Status == OrderStatus.Recollecting
            && (order.Status == OrderStatus.Created || order.Status == OrderStatus.Paid);
        var isValidCancellation = request.Status == OrderStatus.Cancelled
            && order.Status != OrderStatus.Completed
            && order.Status != OrderStatus.Cancelled;

        if (!isSequentialTransition && !isValidRecollectingTransition && !isValidCancellation)
            return false;

        order.Status = request.Status;

        if (request.Status == OrderStatus.Recollecting)
            order.RecollectedAt = DateTime.UtcNow;

        if (request.Status == OrderStatus.Completed)
            order.DeliveredAt = DateTime.UtcNow;

        var notificationEventType = request.Status switch
        {
            OrderStatus.Recollecting => ClientOrderSmsNotificationEventType.PickupOnTheWay,
            OrderStatus.Delivering => ClientOrderSmsNotificationEventType.DeliveryOnTheWay,
            _ => (ClientOrderSmsNotificationEventType?)null
        };

        if (notificationEventType.HasValue && !string.IsNullOrWhiteSpace(order.UserPhone))
        {
            _context.ClientOrderSmsNotificationOutbox.Add(new ClientOrderSmsNotificationOutbox
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                EventType = notificationEventType.Value,
                PhoneNumber = order.UserPhone,
                CustomerName = order.UserName,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<OrderEvidence> AddOrderEvidenceAsync(OrderEvidence evidence, CancellationToken cancellationToken = default)
    {
        await _context.OrderEvidences.AddAsync(evidence, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return evidence;
    }

    public async Task<OrderEvidence?> GetOrderEvidenceByIdAsync(Guid evidenceId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderEvidences
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == evidenceId, cancellationToken);
    }

    public async Task<List<OrderEvidence>> GetOrderEvidencesByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderEvidences
            .Where(x => x.OrderId == orderId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }


}
