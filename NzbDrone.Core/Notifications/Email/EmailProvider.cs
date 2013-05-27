using System;
using System.Net;
using System.Net.Mail;
using NLog;

namespace NzbDrone.Core.Notifications.Email
{
    public class EmailProvider
    {
        private readonly Logger _logger;

        public EmailProvider(Logger logger)
        {
            _logger = logger;
        }

        public virtual void SendEmail(EmailSettings settings, string subject, string body, bool htmlBody = false)
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
                Send(email, settings.Server, settings.Port, settings.UseSsl, credentials);
            }
            catch(Exception ex)
            {
                _logger.Error("Error sending email. Subject: {0}", email.Subject);
                _logger.TraceException(ex.Message, ex);
            }
        }

        public virtual void Send(MailMessage email, string server, int port, bool ssl, NetworkCredential credentials)
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
    }
}
