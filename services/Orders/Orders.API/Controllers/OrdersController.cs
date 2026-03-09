using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
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
    private readonly IFileStorageService _fileStorageService;

    public OrdersController(OrderService orderService, IFileStorageService fileStorageService)
    {
        _orderService = orderService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var orders = await _orderService.GetAllAsync(cancellationToken);
        return Ok(new { message = "Orders retrieved successfully", data = orders });
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetById(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var resp = await _orderService.GetByIdAsync(orderId, cancellationToken);
        var respDetails = await _orderService.GetOrderDetailsByOrderId(orderId, cancellationToken);
        if (resp == null)
            return NotFound(new { message = "Order not found" });
        return Ok(new { message = "Order retrieved successfully", order = resp, orderDetails = respDetails });
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

    [HttpPost("{orderId}/evidences")]
    [ProducesResponseType(typeof(UploadEvidenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadOrderEvidence(
        Guid orderId,
        [FromForm] UploadEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.File == null)
            return BadRequest(new { message = "File is required" });

        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        try
        {
            var storedFile = await _fileStorageService.SaveOrderEvidenceAsync(request.File);

            var evidence = new OrderEvidence
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                CourierId = request.CourierId,
                FileUrl = storedFile.PublicUrl,
                RelativePath = storedFile.RelativePath,
                MimeType = string.IsNullOrWhiteSpace(storedFile.MimeType) ? "application/octet-stream" : storedFile.MimeType,
                SizeBytes = storedFile.SizeBytes,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow
            };

            var createdEvidence = await _orderService.AddOrderEvidenceAsync(evidence, cancellationToken);

            return Ok(new UploadEvidenceResponse
            {
                Message = "Evidence uploaded",
                Evidence = ToEvidenceResponse(createdEvidence)
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error uploading evidence" });
        }
    }

    [HttpGet("{orderId}/evidences")]
    [ProducesResponseType(typeof(IEnumerable<OrderEvidenceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderEvidences(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        var evidences = await _orderService.GetOrderEvidencesByOrderIdAsync(orderId, cancellationToken);
        var response = evidences.Select(ToEvidenceResponse);
        return Ok(response);
    }

    private static OrderEvidenceResponse ToEvidenceResponse(OrderEvidence evidence)
    {
        return new OrderEvidenceResponse
        {
            Id = evidence.Id,
            OrderId = evidence.OrderId,
            CourierId = evidence.CourierId,
            FileUrl = evidence.FileUrl,
            RelativePath = evidence.RelativePath,
            MimeType = evidence.MimeType,
            SizeBytes = evidence.SizeBytes,
            Note = evidence.Note,
            CreatedAt = evidence.CreatedAt
        };
    }
}

