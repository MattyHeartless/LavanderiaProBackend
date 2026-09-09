using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Notifications.API.Data;
using Notifications.API.Models;
using System.Net;
using System.Text.Json;
using WebPush;

namespace Notifications.API.Services;

public sealed class VapidOptions
{
    public string Subject { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}

public sealed class WebPushService(NotificationsDbContext db, IOptions<VapidOptions> options, ILogger<WebPushService> logger)
{
    private readonly VapidOptions _vapid = options.Value;

    public string GetPublicKey()
    {
        if (string.IsNullOrWhiteSpace(_vapid.PublicKey))
            throw new InvalidOperationException("VAPID public key is not configured.");
        return _vapid.PublicKey;
    }

    public async Task UpsertAsync(string authUserId, PushSubscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Endpoint) || string.IsNullOrWhiteSpace(request.Keys.P256dh) || string.IsNullOrWhiteSpace(request.Keys.Auth))
            throw new InvalidOperationException("A complete push subscription is required.");

        var subscription = await db.CourierPushSubscriptions.SingleOrDefaultAsync(x => x.Endpoint == request.Endpoint);
        if (subscription is null)
        {
            subscription = new CourierPushSubscription { Id = Guid.NewGuid(), Endpoint = request.Endpoint, CreatedAt = DateTime.UtcNow };
            db.CourierPushSubscriptions.Add(subscription);
        }

        subscription.AuthUserId = authUserId;
        subscription.P256dh = request.Keys.P256dh;
        subscription.Auth = request.Keys.Auth;
        subscription.IsEnabled = true;
        subscription.LastSeenAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task SetAvailabilityAsync(string authUserId, bool isAvailable)
    {
        var subscriptions = await db.CourierPushSubscriptions.Where(x => x.AuthUserId == authUserId && x.IsEnabled).ToListAsync();
        foreach (var subscription in subscriptions) subscription.IsAvailable = isAvailable;
        await db.SaveChangesAsync();
    }

    public async Task SendNewOrderAsync(Guid orderId)
    {
        EnsureVapidConfigured();
        var subscriptions = await db.CourierPushSubscriptions.Where(x => x.IsAvailable && x.IsEnabled).ToListAsync();
        var payload = JsonSerializer.Serialize(new { title = "Nueva recolección disponible", body = "Hay un pedido listo para tomar.", url = "/app/pedidos", tag = $"order-{orderId}", renotify = true });
        var client = new WebPushClient();
        var vapid = new VapidDetails(_vapid.Subject, _vapid.PublicKey, _vapid.PrivateKey);

        foreach (var record in subscriptions)
        {
            try
            {
                await client.SendNotificationAsync(new PushSubscription(record.Endpoint, record.P256dh, record.Auth), payload, vapid);
                record.LastSeenAt = DateTime.UtcNow;
            }
            catch (WebPushException exception) when (exception.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
            {
                record.IsEnabled = false;
                record.IsAvailable = false;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Could not send push notification to subscription {SubscriptionId}", record.Id);
            }
        }
        await db.SaveChangesAsync();
    }

    private void EnsureVapidConfigured()
    {
        if (string.IsNullOrWhiteSpace(_vapid.Subject) || string.IsNullOrWhiteSpace(_vapid.PublicKey) || string.IsNullOrWhiteSpace(_vapid.PrivateKey))
            throw new InvalidOperationException("VAPID keys are not configured.");
    }
}
