using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.Models;
using Notifications.API.Services;
using System.Security.Claims;

namespace Notifications.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class NotificationsController(WebPushService pushService, SmsNotificationService smsNotificationService, IConfiguration configuration) : ControllerBase
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
            return Unauthorized();
        if (request.OrderId == Guid.Empty)
            return BadRequest(new { message = "OrderId is required" });

        await smsNotificationService.QueueNewOrderAsync(request.OrderId, HttpContext.RequestAborted);
        await pushService.SendNewOrderAsync(request.OrderId);
        return Accepted();
    }

    private string GetAuthUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}
