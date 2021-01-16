using System;
using System.Linq;
using FluentValidation.Results;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NLog;

namespace NzbDrone.Core.Notifications.Email
{
    public interface IEmailService
    {
        void SendEmail(EmailSettings settings, string subject, string body, bool htmlBody = false);
        ValidationFailure Test(EmailSettings settings);
    }

    public class EmailService : IEmailService
    {
        private readonly Logger _logger;

        public EmailService(Logger logger)
        {
            _logger = logger;
        }

        public void SendEmail(EmailSettings settings, string subject, string body, bool htmlBody = false)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(settings.From));

            email.To.Add(MailboxAddress.Parse(settings.To));
            
            email.Subject = subject;
            email.Body = new TextPart(htmlBody ? "html" : "plain")
            {
                Text = body
            };
            
            try
            {
                Send(email, settings);
            }
            catch(Exception ex)
            {
                _logger.Error("Error sending email. Subject: {0}", email.Subject);
                _logger.Debug(ex, ex.Message);
                throw;
            }
        }

        private void Send(MimeMessage email, EmailSettings settings)
        {
            using (var client = new SmtpClient())
            {
                var serverOption = SecureSocketOptions.Auto;

                if (settings.RequireEncryption)
                {
                    if (settings.Port == 465)
                    {
                        serverOption = SecureSocketOptions.SslOnConnect;
                    }
                    else
                    {
                        serverOption = SecureSocketOptions.StartTls;
                    }
                }

                client.Connect(settings.Server, settings.Port, serverOption);

                if (!string.IsNullOrWhiteSpace(settings.Username))
                {
                    client.Authenticate(settings.Username, settings.Password);
                }

                client.Send(email);
                client.Disconnect(true);
            }
        }

        public ValidationFailure Test(EmailSettings settings)
        {
            const string body = "Success! You have properly configured your email notification settings";

            try
            {
                SendEmail(settings, "Sonarr - Test Notification", body);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test email");
                return new ValidationFailure("Server", "Unable to send test email");
            }

            return null;
        }
    }
}
