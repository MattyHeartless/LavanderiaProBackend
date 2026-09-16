using Microsoft.EntityFrameworkCore;
using Notifications.API.Data;
using Notifications.API.Models;

namespace Notifications.API.Services;

public sealed class SmsNotificationService(NotificationsDbContext db, CourierDirectoryClient courierDirectory, SmsGatewayClient smsGateway, ILogger<SmsNotificationService> logger)
{
    private const string CourierNewOrderEventType = "CourierNewOrder";
    private const string PickupOnTheWayEventType = "PickupOnTheWay";
    private const string DeliveryOnTheWayEventType = "DeliveryOnTheWay";
    private const string NewOrderMessage = "Nueva recolección disponible. Abre Reparto para verla y asignártela.";

    public async Task QueueNewOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        if (!smsGateway.IsEnabled)
        {
            logger.LogWarning("SMS notifications are disabled; order {OrderId} will not receive SMS notifications", orderId);
            return;
        }

        logger.LogInformation("Looking up active and available couriers for SMS notification of order {OrderId}", orderId);
        var couriers = await courierDirectory.GetAvailableCouriersAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var queued = 0;

        foreach (var courier in couriers)
        {
            var phoneNumber = NormalizeMexicanPhoneNumber(courier.PhoneNumber);
            if (phoneNumber is null)
            {
                logger.LogWarning("Courier {CourierId} has an invalid SMS phone number", courier.Id);
                continue;
            }

            var alreadyQueued = await db.SmsNotificationOutbox
                .AnyAsync(item => item.OrderId == orderId && item.EventType == CourierNewOrderEventType && item.CourierId == courier.Id, cancellationToken);
            if (alreadyQueued)
                continue;

            db.SmsNotificationOutbox.Add(new SmsNotificationOutbox
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                CourierId = courier.Id,
                EventType = CourierNewOrderEventType,
                PhoneNumber = phoneNumber,
                Message = NewOrderMessage,
                CreatedAt = now,
                NextAttemptAt = now
            });
            queued++;
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Queued {QueuedSmsCount} SMS notification(s) for order {OrderId} from {AvailableCourierCount} available courier(s)", queued, orderId, couriers.Count);
    }

    public async Task QueueClientOrderStatusAsync(ClientOrderStatusSmsNotification request, CancellationToken cancellationToken)
    {
        if (!smsGateway.IsEnabled)
        {
            logger.LogWarning("SMS notifications are disabled; client status event {EventType} for order {OrderId} will not be sent", request.EventType, request.OrderId);
            return;
        }

        var phoneNumber = NormalizeMexicanPhoneNumber(request.PhoneNumber);
        if (phoneNumber is null)
        {
            logger.LogWarning("Client status event {EventType} for order {OrderId} was skipped because the phone number is invalid", request.EventType, request.OrderId);
            return;
        }

        var message = GetClientStatusMessage(request.EventType);
        if (message is null)
            throw new ArgumentException("Unsupported client SMS event type.", nameof(request));

        var alreadyQueued = await db.SmsNotificationOutbox.AnyAsync(item =>
            item.OrderId == request.OrderId && item.EventType == request.EventType && item.CourierId == null,
            cancellationToken);
        if (alreadyQueued)
            return;

        var now = DateTime.UtcNow;
        db.SmsNotificationOutbox.Add(new SmsNotificationOutbox
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            EventType = request.EventType,
            PhoneNumber = phoneNumber,
            Message = message,
            CreatedAt = now,
            NextAttemptAt = now
        });
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Queued client SMS status event {EventType} for order {OrderId}", request.EventType, request.OrderId);
    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        if (!smsGateway.IsEnabled)
            return;

        var now = DateTime.UtcNow;
        var pending = await db.SmsNotificationOutbox
            .Where(item => item.SentAt == null && item.NextAttemptAt <= now && item.Attempts < smsGateway.MaxAttempts)
            .OrderBy(item => item.CreatedAt)
            .Take(25)
            .ToListAsync(cancellationToken);

        foreach (var item in pending)
        {
            item.Attempts++;
            try
            {
                await smsGateway.SendAsync(item.PhoneNumber, item.Message, cancellationToken);
                item.SentAt = DateTime.UtcNow;
                item.LastError = null;
                logger.LogInformation("Sent SMS notification {EventType} for order {OrderId} to {Recipient}", item.EventType, item.OrderId, item.CourierId?.ToString() ?? "client");
            }
            catch (Exception exception)
            {
                item.LastError = exception.Message[..Math.Min(exception.Message.Length, 2000)];
                item.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Min(30, Math.Pow(2, item.Attempts)));
                logger.LogWarning(exception, "Could not send SMS notification {EventType} for order {OrderId} to {Recipient}", item.EventType, item.OrderId, item.CourierId?.ToString() ?? "client");
            }
        }

        if (pending.Count > 0)
            await db.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeMexicanPhoneNumber(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length == 10)
            return $"+52{digits}";
        if (digits.Length == 12 && digits.StartsWith("52", StringComparison.Ordinal))
            return $"+{digits}";
        if (phoneNumber.TrimStart().StartsWith('+') && digits.Length is >= 8 and <= 15)
            return $"+{digits}";
        return null;
    }

    private static string? GetClientStatusMessage(string eventType) => eventType switch
    {
        PickupOnTheWayEventType => "Lavandería a tu casa: ya vamos en camino para recoger tu ropa. Te esperamos pronto.",
        DeliveryOnTheWayEventType => "Lavandería a tu casa: tu ropa limpia ya va de regreso a tu domicilio. Te avisaremos al llegar.",
        _ => null
    };
}
