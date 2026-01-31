using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EmailWorker
{

    public sealed class SmtpSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string From { get; set; } = default!;
    }

    public interface IEmailSender
    {
        Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct);
    }

    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(SmtpSettings settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_settings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient
            {
                Timeout = 100000 // 100s; make it explicit
            };

            try
            {
                _logger.LogInformation("SMTP connecting to {Host}:{Port}...", _settings.Host, _settings.Port);
                await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect, ct);

                _logger.LogInformation("SMTP authenticating as {User}...", _settings.Username);
                await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);

                _logger.LogInformation("SMTP sending to {To}...", to);
                await client.SendAsync(message, ct);

                _logger.LogInformation("SMTP sent to {To}. Disconnecting...", to);
                await client.DisconnectAsync(true, ct);
            }
            catch
            {
                try { if (client.IsConnected) await client.DisconnectAsync(true, ct); } catch { /* ignore */ }
                throw;
            }
        }
    }
}
