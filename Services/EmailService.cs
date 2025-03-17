using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // Add this

namespace MormorsBageri.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger; // Add this

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? _config["Email:FromEmail"];
            var smtpHost = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST") ?? _config["Email:SmtpHost"];
            var smtpPortStr = Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT") ?? _config["Email:SmtpPort"];
            var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? _config["Email:Username"];
            var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? _config["Email:Password"];

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || !int.TryParse(smtpPortStr, out int smtpPort))
            {
                _logger.LogWarning("Email configuration is incomplete. Email not sent to {ToEmail}.", toEmail);
                return; // Silent fail with logging instead of throwing
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Mormors Bageri", fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully to {ToEmail}.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}.", toEmail);
            }
        }
    }
}