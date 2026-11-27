using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MyWarehouse.WebApi.Email
{
    public interface IOutboxEmailSender
    {
        Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct);
    }

    /// <summary>
    /// Sends email through Resend's REST API (POST https://api.resend.com/emails).
    /// Uses plain HttpClient so the API needs no extra NuGet package.
    /// Config: Resend:ApiKey (secret), Email:From (sender address).
    /// </summary>
    public class ResendHttpEmailSender : IOutboxEmailSender
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<ResendHttpEmailSender> _logger;

        public ResendHttpEmailSender(HttpClient http, IConfiguration config, ILogger<ResendHttpEmailSender> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        public async Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct)
        {
            var apiKey = _config["Resend:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Missing Resend:ApiKey.");
            }

            var from = _config["Email:From"];
            if (string.IsNullOrWhiteSpace(from))
            {
                from = "onboarding@resend.dev";
            }

            var payload = JsonSerializer.Serialize(new
            {
                from = $"Property Master <{from}>",
                to = new[] { to },
                subject,
                html = htmlBody
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Resend returned {(int)response.StatusCode}: {body}");
            }

            _logger.LogInformation("Email sent via Resend to {To} (subject '{Subject}')", to, subject);
        }
    }
}
