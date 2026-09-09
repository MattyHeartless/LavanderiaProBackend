using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Services;

public sealed class OrderNotificationOutboxWorker(IServiceScopeFactory scopeFactory, ILogger<OrderNotificationOutboxWorker> logger) : BackgroundService
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
                var pending = await db.OrderNotificationOutbox
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(25)
                    .ToListAsync(stoppingToken);

                foreach (var message in pending)
                {
                    message.Attempts++;
                    if (await publisher.PublishUnassignedOrderAsync(message.OrderId, stoppingToken))
                        message.ProcessedAt = DateTime.UtcNow;
                }
                if (pending.Count > 0) await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to process the orders notification outbox");
            }
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
