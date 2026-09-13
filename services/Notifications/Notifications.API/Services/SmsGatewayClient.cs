using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;

namespace Notifications.API.Services;

public sealed class SmsGatewayOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://api.sms-gate.app/";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int MaxAttempts { get; set; } = 5;
}

public sealed class SmsGatewayClient(HttpClient httpClient, IOptions<SmsGatewayOptions> options)
{
    private readonly SmsGatewayOptions _options = options.Value;

    public bool IsEnabled => _options.Enabled;
    public int MaxAttempts => Math.Max(1, _options.MaxAttempts);

    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password))
            throw new InvalidOperationException("SMS gateway credentials are not configured.");

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
        using var request = new HttpRequestMessage(HttpMethod.Post, "3rdparty/v1/messages")
        {
            Content = JsonContent.Create(new
            {
                textMessage = new { text = message },
                phoneNumbers = new[] { phoneNumber }
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"SMS gateway returned {(int)response.StatusCode}: {responseBody}");
        }
    }
}
