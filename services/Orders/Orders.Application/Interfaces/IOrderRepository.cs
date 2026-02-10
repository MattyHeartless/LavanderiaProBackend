using Orders.Domain.Entities;

namespace Orders.Application.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Order>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<Guid> AddAsync(Order order,IEnumerable<OrderDetail> orderDetails,CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task AddDetailAsync(OrderDetail detail, CancellationToken cancellationToken);
}
