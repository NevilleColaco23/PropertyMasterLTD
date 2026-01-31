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
        public SmtpEmailSender(SmtpSettings settings) => _settings = settings;

        public async Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_settings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
