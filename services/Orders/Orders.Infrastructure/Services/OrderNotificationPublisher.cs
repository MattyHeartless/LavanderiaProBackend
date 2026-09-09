using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Orders.Infrastructure.Services;

public interface IOrderNotificationPublisher
{
    Task<bool> PublishUnassignedOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public sealed class OrderNotificationPublisher(HttpClient httpClient, IConfiguration configuration, ILogger<OrderNotificationPublisher> logger) : IOrderNotificationPublisher
{
    public async Task<bool> PublishUnassignedOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "internal/unassigned-order")
            {
                Content = JsonContent.Create(new { orderId })
            };
            request.Headers.Add("X-Internal-Api-Key", configuration["Notifications:InternalApiKey"] ?? string.Empty);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "New order {OrderId} was saved but its push notification could not be dispatched", orderId);
            return false;
        }
    }
}
