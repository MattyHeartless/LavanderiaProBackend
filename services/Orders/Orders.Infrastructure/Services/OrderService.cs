using Orders.Application.DTOs;
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

    public async Task<IEnumerable<RetrieveOrders>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<RetrieveOrders>> GetUnassignedAsync(CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetUnassignedAsync(cancellationToken);
    }

    public async Task<IEnumerable<RetrieveOrders>> GetByUserIdAsync(
    string userId, 
    CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<IEnumerable<RetrieveOrders>> GetByCourierGuidAsync(
        Guid courierGuid,
        CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetByCourierGuidAsync(courierGuid, cancellationToken);
    }

    public async Task<CourierWorkedOrdersKpisResponse> GetCourierWorkedOrdersKpisAsync(
        Guid courierGuid,
        CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetCourierWorkedOrdersKpisAsync(courierGuid, cancellationToken);
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

    public async Task<bool> AssignCourierAsync(
        Guid orderId,
        AssignOrderCourierRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _orderRepository.AssignCourierAsync(orderId, request, cancellationToken);
    }

    public async Task<bool> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _orderRepository.UpdateStatusAsync(orderId, request, cancellationToken);
    }

    public async Task AddDetailAsync(OrderDetail detail, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<List<OrderDetail>> GetOrderDetailsByOrderId(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetOrderDetailsByOrderId(orderId, cancellationToken);
    }

    public async Task<OrderEvidence> AddOrderEvidenceAsync(OrderEvidence evidence, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.AddOrderEvidenceAsync(evidence, cancellationToken);
    }

    public async Task<OrderEvidence?> GetOrderEvidenceByIdAsync(Guid evidenceId, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetOrderEvidenceByIdAsync(evidenceId, cancellationToken);
    }

    public async Task<List<OrderEvidence>> GetOrderEvidencesByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetOrderEvidencesByOrderIdAsync(orderId, cancellationToken);
    }
}