using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace MormorsBageri.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailAddress = new EmailAddressAttribute();
                return emailAddress.IsValid(email);
            }
            catch
            {
                return false;
            }
        }

        // Synchronous method to match the call in AuthController.cs
        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                SendEmailAsync(toEmail, subject, body).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}. Error: {ex.Message}", ex);
                throw;
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentNullException(nameof(toEmail), "Recipient email cannot be null or empty.");

            if (!IsValidEmail(toEmail))
                throw new ArgumentException($"Invalid email address: {toEmail}", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentNullException(nameof(subject), "Subject cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException(nameof(body), "Body cannot be null or empty.");

            try
            {
                var smtpServer = _configuration["Email:SmtpHost"];
                var smtpPortString = _configuration["Email:SmtpPort"];
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var fromEmail = _configuration["Email:FromEmail"];

                if (string.IsNullOrWhiteSpace(smtpServer))
                    throw new InvalidOperationException("SMTP host is not configured (Email:SmtpHost).");
                if (string.IsNullOrWhiteSpace(smtpPortString) || !int.TryParse(smtpPortString, out int smtpPort))
                    throw new InvalidOperationException("SMTP port is not configured or invalid (Email:SmtpPort).");
                if (string.IsNullOrWhiteSpace(smtpUsername))
                    throw new InvalidOperationException("SMTP username is not configured (Email:Username).");
                if (string.IsNullOrWhiteSpace(smtpPassword))
                    throw new InvalidOperationException("SMTP password is not configured (Email:Password).");
                if (string.IsNullOrWhiteSpace(fromEmail))
                    throw new InvalidOperationException("From email is not configured (Email:FromEmail).");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Mormors Bageri", fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { TextBody = body };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    _logger.LogInformation($"Connecting to SMTP server {smtpServer}:{smtpPort}");
                    await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    _logger.LogInformation($"Authenticating with username {smtpUsername}");
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    _logger.LogInformation($"Sending email to {toEmail} with subject: {subject}");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Email successfully sent to {toEmail} with subject: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}. Error: {ex.Message}", ex);
                throw;
            }
        }
    }
}