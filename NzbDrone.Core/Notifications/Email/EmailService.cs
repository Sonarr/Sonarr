using System;
using System.Net;
using System.Net.Mail;
using NLog;
using NzbDrone.Common.Messaging;
using Omu.ValueInjecter;

namespace NzbDrone.Core.Notifications.Email
{
    public interface IEmailService
    {
        void SendEmail(EmailSettings settings, string subject, string body, bool htmlBody = false);
    }

    public class EmailService : IEmailService, IExecute<TestEmailCommand>
    {
        private readonly Logger _logger;

        public EmailService(Logger logger)
        {
            _logger = logger;
        }

        public void SendEmail(EmailSettings settings, string subject, string body, bool htmlBody = false)
        {
            var email = new MailMessage();
            email.From = new MailAddress(settings.From);
            
            email.To.Add(settings.To);

            email.Subject = subject;
            email.Body = body;
            email.IsBodyHtml = htmlBody;

            NetworkCredential credentials = null;

            if (!String.IsNullOrWhiteSpace(settings.Username))
                credentials = new NetworkCredential(settings.Username, settings.Password);

            try
            {
                Send(email, settings.Server, settings.Port, settings.Ssl, credentials);
            }
            catch(Exception ex)
            {
                _logger.Error("Error sending email. Subject: {0}", email.Subject);
                _logger.TraceException(ex.Message, ex);
            }
        }

        private void Send(MailMessage email, string server, int port, bool ssl, NetworkCredential credentials)
        {
            try
            {
                var smtp = new SmtpClient(server, port);

                smtp.EnableSsl = ssl;

                smtp.Credentials = credentials;

                smtp.Send(email);
            }

            catch (Exception ex)
            {
                _logger.ErrorException("There was an error sending an email.", ex);
                throw;
            }
        }

        public void Execute(TestEmailCommand message)
        {
            var settings = new EmailSettings();
            settings.InjectFrom(message);

            const string body = "Success! You have properly configured your email notification settings";

            SendEmail(settings, "NzbDrone - Test Notification", body);
        }
    }
}
