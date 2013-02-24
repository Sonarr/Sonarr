using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using NLog;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Providers
{
    public class SmtpProvider
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public SmtpProvider(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public virtual void SendEmail(string subject, string body, bool htmlBody = false)
        {
            var email = new MailMessage();
            email.From = new MailAddress(_configService.SmtpFromAddress);
            
            foreach (var toAddress in _configService.SmtpToAddresses.Split(','))
            {
                email.To.Add(toAddress.Trim());
            }

            email.Subject = subject;
            email.Body = body;
            email.IsBodyHtml = htmlBody;
            
            var username = _configService.SmtpUsername;
            var password = _configService.SmtpPassword;

            NetworkCredential credentials = null;

            if (!String.IsNullOrWhiteSpace(username))
                credentials = new NetworkCredential(username, password);

            try
            {
                Send(email, _configService.SmtpServer, _configService.SmtpPort, _configService.SmtpUseSsl, credentials);
            }
            catch(Exception ex)
            {
                _logger.Error("Error sending email. Subject: {0}", email.Subject);
                _logger.TraceException(ex.Message, ex);
            }
        }

        public virtual bool SendTestEmail(string server, int port, bool ssl, string username, string password, string fromAddress, string toAddresses)
        {
            var subject = "NzbDrone SMTP Test Notification";
            var body = "This is a test email from NzbDrone, if you received this message you properly configured your SMTP settings! (Now save them!)";

            var email = new MailMessage();

            email.From = new MailAddress(fromAddress);

            foreach (var toAddress in toAddresses.Split(','))
            {
                email.To.Add(toAddress.Trim());
            }

            email.Subject = subject;

            email.Body = body;

            email.IsBodyHtml = false;

            NetworkCredential credentials = null;

            if (!String.IsNullOrWhiteSpace(username))
                credentials = new NetworkCredential(username, password);

            try
            {
                Send(email, server, port, ssl, credentials);
            }
            catch(Exception ex)
            {
                _logger.TraceException("Failed to send test email", ex);
                return false;
            }
            return true;
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
