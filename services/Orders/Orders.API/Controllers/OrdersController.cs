using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs;
using Orders.Application.Repositories;
using Orders.Domain.Entities;
using Orders.Infrastructure.Services;


namespace Orders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetById(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return NotFound(new { message = "Order not found" });
        return Ok(new { message = "Order retrieved successfully", data = order });
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(new { message = "Orders retrieved successfully", data = orders });
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        Guid result = await _orderService.AddAsync(request.Order, request.OrderDetails, cancellationToken);
        return Ok(new { message = "Order created successfully", orderId = result });
    }

    [HttpPut("{orderId}")]
    public async Task<IActionResult> Update(
        Guid orderId,
        [FromBody] Order order,
        CancellationToken cancellationToken = default)
    {
        order.Id = orderId;
        await _orderService.UpdateAsync(order, cancellationToken);
        return Ok(new { message = "Order updated successfully" });
    }
}

