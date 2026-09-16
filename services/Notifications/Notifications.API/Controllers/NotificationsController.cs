using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.Models;
using Notifications.API.Services;
using System.Security.Claims;

namespace Notifications.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class NotificationsController(WebPushService pushService, SmsNotificationService smsNotificationService, IConfiguration configuration, ILogger<NotificationsController> logger) : ControllerBase
{
    [Authorize(Roles = "Courier")]
    [HttpGet("push/public-key")]
    public IActionResult GetPublicKey() => Ok(new { publicKey = pushService.GetPublicKey() });

    [Authorize(Roles = "Courier")]
    [HttpPost("push/subscriptions")]
    public async Task<IActionResult> SaveSubscription(PushSubscriptionRequest request)
    {
        await pushService.UpsertAsync(GetAuthUserId(), request);
        return NoContent();
    }

    [Authorize(Roles = "Courier")]
    [HttpPatch("push/availability")]
    public async Task<IActionResult> SetAvailability(AvailabilityRequest request)
    {
        await pushService.SetAvailabilityAsync(GetAuthUserId(), request.IsAvailable);
        return NoContent();
    }

    [HttpPost("internal/unassigned-order")]
    public async Task<IActionResult> NotifyNewUnassignedOrder(NewUnassignedOrderNotification request, [FromHeader(Name = "X-Internal-Api-Key")] string? internalApiKey)
    {
        var expectedInternalApiKey = configuration["InternalApi:Key"];
        if (string.IsNullOrWhiteSpace(expectedInternalApiKey) || !string.Equals(internalApiKey, expectedInternalApiKey, StringComparison.Ordinal))
        {
            logger.LogWarning("Rejected internal new-order notification because its internal API key is invalid or not configured");
            return Unauthorized();
        }
        if (request.OrderId == Guid.Empty)
            return BadRequest(new { message = "OrderId is required" });

        logger.LogInformation("Received new-order notification for order {OrderId}; resolving SMS recipients", request.OrderId);
        await smsNotificationService.QueueNewOrderAsync(request.OrderId, HttpContext.RequestAborted);
        try
        {
            await pushService.SendNewOrderAsync(request.OrderId);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Push notification failed for order {OrderId}; SMS processing will continue independently", request.OrderId);
        }
        return Accepted();
    }

    [HttpPost("internal/client-order-status")]
    public async Task<IActionResult> NotifyClientOrderStatus(ClientOrderStatusSmsNotification request, [FromHeader(Name = "X-Internal-Api-Key")] string? internalApiKey)
    {
        var expectedInternalApiKey = configuration["InternalApi:Key"];
        if (string.IsNullOrWhiteSpace(expectedInternalApiKey) || !string.Equals(internalApiKey, expectedInternalApiKey, StringComparison.Ordinal))
        {
            logger.LogWarning("Rejected internal client status notification because its internal API key is invalid or not configured");
            return Unauthorized();
        }
        if (request.OrderId == Guid.Empty || string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(new { message = "OrderId and PhoneNumber are required" });

        if (request.EventType is not "PickupOnTheWay" and not "DeliveryOnTheWay")
            return BadRequest(new { message = "Unsupported client status notification event type" });

        await smsNotificationService.QueueClientOrderStatusAsync(request, HttpContext.RequestAborted);
        return Accepted();
    }

    private string GetAuthUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}
