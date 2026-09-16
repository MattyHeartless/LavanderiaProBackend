using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Services;

public sealed class ClientOrderSmsNotificationOutboxWorker(IServiceScopeFactory scopeFactory, ILogger<ClientOrderSmsNotificationOutboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IOrderNotificationPublisher>();
                var pending = await db.ClientOrderSmsNotificationOutbox
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(25)
                    .ToListAsync(stoppingToken);

                foreach (var notification in pending)
                {
                    notification.Attempts++;
                    if (await publisher.PublishClientOrderStatusAsync(notification, stoppingToken))
                        notification.ProcessedAt = DateTime.UtcNow;
                }

                if (pending.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to process the client order SMS notification outbox");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
