using Orders.Application.DTOs;
using Orders.Domain.Entities;

namespace Orders.Application.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<RetrieveOrders>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<RetrieveOrders>> GetUnassignedAsync(CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IEnumerable<RetrieveOrders>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<RetrieveOrders>> GetByCourierGuidAsync(Guid courierGuid, CancellationToken cancellationToken = default);
    Task<CourierWorkedOrdersKpisResponse> GetCourierWorkedOrdersKpisAsync(Guid courierGuid, CancellationToken cancellationToken = default);
    Task<List<DeliveryMode>> GetActiveDeliveryModesAsync(CancellationToken cancellationToken = default);
    Task<DeliveryMode?> GetDeliveryModeByIdAsync(int deliveryModeId, CancellationToken cancellationToken = default);

    Task<Guid> AddAsync(Order order,IEnumerable<OrderDetail> orderDetails,CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> AssignCourierAsync(Guid orderId, AssignOrderCourierRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task AddDetailAsync(OrderDetail detail, CancellationToken cancellationToken);
    Task<List<OrderDetail>> GetOrderDetailsByOrderId(Guid orderId,CancellationToken cancellationToken = default);
    Task<OrderEvidence> AddOrderEvidenceAsync(OrderEvidence evidence, CancellationToken cancellationToken = default);
    Task<OrderEvidence?> GetOrderEvidenceByIdAsync(Guid evidenceId, CancellationToken cancellationToken = default);
    Task<List<OrderEvidence>> GetOrderEvidencesByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}