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
    private const int DefaultDeliveryModeId = 4;

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

    [HttpGet("unassigned")]
    public async Task<IActionResult> GetUnassigned(CancellationToken cancellationToken = default)
    {
        var orders = await _orderService.GetUnassignedAsync(cancellationToken);
        return Ok(new { message = "Unassigned orders retrieved successfully", data = orders });
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

    [HttpGet("courier/{courierGuid}")]
    public async Task<IActionResult> GetByCourierGuid(
        Guid courierGuid,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderService.GetByCourierGuidAsync(courierGuid, cancellationToken);
        return Ok(new { message = "Orders retrieved successfully", data = orders });
    }

    [HttpGet("courier/{courierGuid}/kpis")]
    public async Task<IActionResult> GetCourierWorkedOrdersKpis(
        Guid courierGuid,
        CancellationToken cancellationToken = default)
    {
        if (courierGuid == Guid.Empty)
            return BadRequest(new { message = "CourierGuid is required" });

        var kpis = await _orderService.GetCourierWorkedOrdersKpisAsync(courierGuid, cancellationToken);
        return Ok(new { message = "Courier KPIs retrieved successfully", data = kpis });
    }

    [HttpGet("delivery-modes")]
    public async Task<IActionResult> GetDeliveryModes(CancellationToken cancellationToken = default)
    {
        var modes = await _orderService.GetActiveDeliveryModesAsync(cancellationToken);
        return Ok(new { message = "Delivery modes retrieved successfully", data = modes });
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.OrderDetails == null || !request.OrderDetails.Any())
            return BadRequest(new { message = "At least one order detail is required" });

        // Validate each order detail
        foreach (var detail in request.OrderDetails)
        {
            var validationError = OrderDetailRules.Validate(detail);
            if (validationError != null)
                return BadRequest(new { message = validationError });
        }

        var selectedDeliveryModeId = request.Order.DeliveryModeId ?? DefaultDeliveryModeId;
        var selectedDeliveryMode = await _orderService.GetDeliveryModeByIdAsync(selectedDeliveryModeId, cancellationToken);
        if (selectedDeliveryMode == null || !selectedDeliveryMode.IsActive)
            return BadRequest(new { message = "Selected delivery mode is invalid or inactive" });

        request.Order.DeliveryModeId = selectedDeliveryMode.Id;
        request.Order.DeliveryModeCode = selectedDeliveryMode.Code;
        request.Order.DeliveryModeName = selectedDeliveryMode.Name;
        request.Order.DeliveryEtaHours = selectedDeliveryMode.EtaHours;
        request.Order.DeliveryModeSurcharge = selectedDeliveryMode.SurchargeAmount;

        var detailsSubtotal = request.OrderDetails.Sum(x => x.SubTotal);
        request.Order.TotalAmount = detailsSubtotal + request.Order.DeliveryFee;

        if (request.Order.Status == 0)
            request.Order.Status = OrderStatus.Created;

        if (request.Order.CreatedAt == default)
            request.Order.CreatedAt = DateTime.UtcNow;

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

    [HttpPatch("{orderId}/assign-courier")]
    public async Task<IActionResult> AssignCourier(
        Guid orderId,
        [FromBody] AssignOrderCourierRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CourierGuid == Guid.Empty)
            return BadRequest(new { message = "CourierGuid is required" });

        if (string.IsNullOrWhiteSpace(request.CourierName))
            return BadRequest(new { message = "CourierName is required" });

        var updated = await _orderService.AssignCourierAsync(orderId, request, cancellationToken);
        if (!updated)
            return NotFound(new { message = "Order not found" });

        return Ok(new { message = "Courier assigned successfully" });
    }

    [HttpPatch("{orderId}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        var updated = await _orderService.UpdateStatusAsync(orderId, request, cancellationToken);
        if (!updated)
            return BadRequest(new
            {
                message = "Invalid status transition. Move the order step-by-step until completion."
            });

        return Ok(new { message = "Order status updated successfully", status = request.Status });
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
                OrderStatusEvidence = order.Status,
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

    [HttpGet("evidences/{evidenceId}/image")]
    public async Task<IActionResult> GetEvidenceImage(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        var evidence = await _orderService.GetOrderEvidenceByIdAsync(evidenceId, cancellationToken);
        if (evidence == null)
            return NotFound(new { message = "Evidence not found" });

        var imagePath = string.IsNullOrWhiteSpace(evidence.FileUrl) ? evidence.RelativePath : evidence.FileUrl;
        return Redirect(imagePath);
    }

    private static OrderEvidenceResponse ToEvidenceResponse(OrderEvidence evidence)
    {
        return new OrderEvidenceResponse
        {
            Id = evidence.Id,
            OrderId = evidence.OrderId,
            OrderStatusEvidence = evidence.OrderStatusEvidence,
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

