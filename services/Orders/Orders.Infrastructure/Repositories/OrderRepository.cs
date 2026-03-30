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
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
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

        if (request.Status == OrderStatus.Delivering)
            order.DeliveredAt = DateTime.UtcNow;

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
