using Resend;

namespace EmailWorker
{
    internal class ResendEmailSender : IEmailSender
    {
        private readonly ResendClient _client;
        private readonly string _from;

        public ResendEmailSender(ResendClient client, IConfiguration config)
        {
            _client = client;
            _from = config["Email:From"] ?? config["EMAIL_FROM"]
                ?? throw new InvalidOperationException("Missing Email:From (or EMAIL_FROM).");
        }

        public async Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct)
        {
            var msg = new EmailMessage
            {
                From = "Property Master <" + _from + ">",
                To = to,
                Subject = subject,
                HtmlBody = htmlBody
            };

            var resp = await _client.EmailSendAsync(msg, ct);

            if (resp.Content == Guid.Empty)
                throw new InvalidOperationException("Resend did not return a message id (send may have failed).");
        }
    }
}