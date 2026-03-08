using Resend;

namespace EmailWorker
{
    internal class ResendEmailSender : IEmailSender
    {
        private readonly ResendClient _client;
        private readonly string _from;
        private readonly ILogger<ResendEmailSender> _logger;

        public ResendEmailSender(ResendClient client, IConfiguration config, ILogger<ResendEmailSender> logger)
        {
            _client = client;
            _logger = logger;
            _from = config["Email:From"] ?? config["EMAIL_FROM"]
                ?? throw new InvalidOperationException("Missing Email:From (or EMAIL_FROM).");

            _logger.LogInformation("ResendEmailSender initialized. From address: {FromAddress}", _from);
        }

        public async Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct)
        {
            // Create plain text version from HTML (fallback for email clients)
            var textBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, "<.*?>", string.Empty);
            textBody = System.Net.WebUtility.HtmlDecode(textBody);

            var msg = new EmailMessage
            {
                From = "Property Master <" + _from + ">",
                To = to,
                Subject = subject,
                HtmlBody = htmlBody,
                TextBody = textBody  // Add plain text fallback
            };

            var resp = await _client.EmailSendAsync(msg, ct);

            if (resp.Content == Guid.Empty)
                throw new InvalidOperationException("Resend did not return a message id (send may have failed).");
        }
    }
}