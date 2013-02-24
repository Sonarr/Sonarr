using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class SmtpProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IConfigService _configService;

        public SmtpProvider(IConfigService configService)
        {
            _configService = configService;
        }

        public virtual void SendEmail(string subject, string body, bool htmlBody = false)
        {
            //Create the Email message
            var email = new MailMessage();

            //Set the addresses
            email.From = new MailAddress(_configService.SmtpFromAddress);

            //Allow multiple to addresses (split on each comma)
            foreach (var toAddress in _configService.SmtpToAddresses.Split(','))
            {
                email.To.Add(toAddress.Trim());
            }

            //Set the Subject
            email.Subject = subject;

            //Set the Body
            email.Body = body;

            //Html Body
            email.IsBodyHtml = htmlBody;

            //Handle credentials
            var username = _configService.SmtpUsername;
            var password = _configService.SmtpPassword;

            NetworkCredential credentials = null;

            if (!String.IsNullOrWhiteSpace(username))
                credentials = new NetworkCredential(username, password);

            //Send the email
            try
            {
                Send(email, _configService.SmtpServer, _configService.SmtpPort, _configService.SmtpUseSsl, credentials);
            }
            catch(Exception ex)
            {
                Logger.Error("Error sending email. Subject: {0}", email.Subject);
                Logger.TraceException(ex.Message, ex);
            }
        }

        public virtual bool SendTestEmail(string server, int port, bool ssl, string username, string password, string fromAddress, string toAddresses)
        {
            var subject = "NzbDrone SMTP Test Notification";
            var body = "This is a test email from NzbDrone, if you received this message you properly configured your SMTP settings! (Now save them!)";

            //Create the Email message
            var email = new MailMessage();

            //Set the addresses
            email.From = new MailAddress(fromAddress);

            //Allow multiple to addresses (split on each comma)
            foreach (var toAddress in toAddresses.Split(','))
            {
                email.To.Add(toAddress.Trim());
            }

            //Set the Subject
            email.Subject = subject;

            //Set the Body
            email.Body = body;

            //Html Body
            email.IsBodyHtml = false;

            //Handle credentials
            NetworkCredential credentials = null;

            if (!String.IsNullOrWhiteSpace(username))
                credentials = new NetworkCredential(username, password);

            //Send the email
            try
            {
                Send(email, server, port, ssl, credentials);
            }
            catch(Exception ex)
            {
                Logger.TraceException("Failed to send test email", ex);
                return false;
            }
            return true;
        }

        public virtual void Send(MailMessage email, string server, int port, bool ssl, NetworkCredential credentials)
        {
            try
            {
                //Create the SMTP connection
                var smtp = new SmtpClient(server, port);

                //Enable SSL
                smtp.EnableSsl = ssl;

                //Credentials
                smtp.Credentials = credentials;

                //Send the email
                smtp.Send(email);
            }

            catch (Exception ex)
            {
                Logger.ErrorException("There was an error sending an email.", ex);
                throw;
            }
        }
    }
}
