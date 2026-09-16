using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Services;

public interface IOrderNotificationPublisher
{
    Task<bool> PublishUnassignedOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> PublishClientOrderStatusAsync(ClientOrderSmsNotificationOutbox notification, CancellationToken cancellationToken = default);
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
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "Notifications rejected the new-order event for order {OrderId}. URL: {Url}; status: {StatusCode}; response: {ResponseBody}",
                    orderId,
                    request.RequestUri,
                    (int)response.StatusCode,
                    responseBody);
                return false;
            }
            response.EnsureSuccessStatusCode();
            logger.LogInformation("Notifications accepted the new-order event for order {OrderId}", orderId);
            return true;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not send the new-order event for order {OrderId} to Notifications at {NotificationsBaseUrl}", orderId, httpClient.BaseAddress);
            return false;
        }
    }

    public async Task<bool> PublishClientOrderStatusAsync(ClientOrderSmsNotificationOutbox notification, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "internal/client-order-status")
            {
                Content = JsonContent.Create(new
                {
                    notification.OrderId,
                    phoneNumber = notification.PhoneNumber,
                    customerName = notification.CustomerName,
                    eventType = notification.EventType.ToString()
                })
            };
            request.Headers.Add("X-Internal-Api-Key", configuration["Notifications:InternalApiKey"] ?? string.Empty);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "Notifications rejected client status event {EventType} for order {OrderId}. URL: {Url}; status: {StatusCode}; response: {ResponseBody}",
                    notification.EventType,
                    notification.OrderId,
                    request.RequestUri,
                    (int)response.StatusCode,
                    responseBody);
                return false;
            }

            logger.LogInformation("Notifications accepted client status event {EventType} for order {OrderId}", notification.EventType, notification.OrderId);
            return true;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not send client status event {EventType} for order {OrderId} to Notifications at {NotificationsBaseUrl}", notification.EventType, notification.OrderId, httpClient.BaseAddress);
            return false;
        }
    }
}
