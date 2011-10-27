using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using NLog;
using Ninject;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class SmtpProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;

        [Inject]
        public SmtpProvider(ConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public virtual bool SendEmail(string subject, string body, bool htmlBody = false)
        {
            //Create the Email message
            var email = new MailMessage();

            //Set the addresses
            email.From = new MailAddress(_configProvider.SmtpFromAddress);

            //Allow multiple to addresses (split on each comma)
            foreach (var toAddress in _configProvider.SmtpToAddresses.Split(','))
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
            var username = _configProvider.SmtpUsername;
            var password = _configProvider.SmtpPassword;

            NetworkCredential credentials = null;

            if (!String.IsNullOrWhiteSpace(username))
                credentials = new NetworkCredential(username, password);

            //Send the email
            return Send(email, _configProvider.SmtpServer, _configProvider.SmtpPort, _configProvider.SmtpUseSsl, credentials);
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
            return Send(email, _configProvider.SmtpServer, _configProvider.SmtpPort, _configProvider.SmtpUseSsl, credentials);
        }

        public virtual bool Send(MailMessage email, string server, int port, bool ssl, NetworkCredential credentials)
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

                return true;
            }

            catch (Exception ex)
            {
                Logger.Error("There was an error sending an email.");
                Logger.TraceException(ex.Message, ex);
                return false;
            }
        }
    }
}
