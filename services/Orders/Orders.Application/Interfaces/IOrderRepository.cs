using Orders.Application.DTOs;
using Orders.Domain.Entities;

namespace Orders.Application.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<RetrieveOrders>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IEnumerable<RetrieveOrders>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<Guid> AddAsync(Order order,IEnumerable<OrderDetail> orderDetails,CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task AddDetailAsync(OrderDetail detail, CancellationToken cancellationToken);
    Task<List<OrderDetail>> GetOrderDetailsByOrderId(Guid orderId,CancellationToken cancellationToken = default);
}