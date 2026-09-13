using Microsoft.EntityFrameworkCore;
using Notifications.API.Data;
using Notifications.API.Models;

namespace Notifications.API.Services;

public sealed class SmsNotificationService(NotificationsDbContext db, CourierDirectoryClient courierDirectory, SmsGatewayClient smsGateway, ILogger<SmsNotificationService> logger)
{
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
                .AnyAsync(item => item.OrderId == orderId && item.CourierId == courier.Id, cancellationToken);
            if (alreadyQueued)
                continue;

            db.SmsNotificationOutbox.Add(new SmsNotificationOutbox
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                CourierId = courier.Id,
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
                logger.LogInformation("Sent SMS notification for order {OrderId} to courier {CourierId}", item.OrderId, item.CourierId);
            }
            catch (Exception exception)
            {
                item.LastError = exception.Message[..Math.Min(exception.Message.Length, 2000)];
                item.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Min(30, Math.Pow(2, item.Attempts)));
                logger.LogWarning(exception, "Could not send SMS for order {OrderId} to courier {CourierId}", item.OrderId, item.CourierId);
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
}
