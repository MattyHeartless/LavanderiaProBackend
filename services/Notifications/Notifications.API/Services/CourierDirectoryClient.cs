using System.Net.Http.Json;

namespace Notifications.API.Services;

public sealed record CourierSmsTarget(Guid Id, string PhoneNumber);

public sealed class CourierDirectoryClient(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<IReadOnlyList<CourierSmsTarget>> GetAvailableCouriersAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "internal/couriers/notification-targets");
        request.Headers.Add("X-Internal-Api-Key", configuration["InternalApi:Key"] ?? string.Empty);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CourierTargetsResponse>(cancellationToken: cancellationToken);
        return payload?.Couriers ?? [];
    }

    private sealed class CourierTargetsResponse
    {
        public List<CourierSmsTarget> Couriers { get; set; } = [];
    }
}
