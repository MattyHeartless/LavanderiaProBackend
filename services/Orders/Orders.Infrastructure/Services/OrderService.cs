using Orders.Application.Repositories;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Services;

public class OrderService : IOrderRepository
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Order?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetByIdAsync(orderId, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<Guid> AddAsync(
        Order order,
        IEnumerable<OrderDetail> orderDetails,
        CancellationToken cancellationToken = default)
    {
        await _orderRepository.AddAsync(order, orderDetails, cancellationToken);
        
        foreach (var detail in orderDetails)
        {
            detail.OrderId = order.Id;
            await _orderRepository.AddDetailAsync(detail, cancellationToken);
        }
        
        return order.Id;
    }

    public async Task UpdateAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        await _orderRepository.UpdateAsync(order, cancellationToken);
    }

    public Task AddDetailAsync(OrderDetail detail, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}