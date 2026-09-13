using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Notifications.API.Services;

public sealed class SmsNotificationOutboxWorker(IServiceScopeFactory scopeFactory, ILogger<SmsNotificationOutboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<SmsNotificationService>();
                await service.ProcessPendingAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to process the SMS notification outbox");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
