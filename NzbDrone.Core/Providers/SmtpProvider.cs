using System;
using System.Collections.Generic;
using System.Linq;
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
            try
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

                //Create the SMTP connection
                var smtp = new SmtpClient(_configProvider.SmtpServer, _configProvider.SmtpPort);

                //Enable SSL
                smtp.EnableSsl = true;

                //Credentials
                smtp.Credentials = new System.Net.NetworkCredential(_configProvider.SmtpUsername, _configProvider.SmtpPassword);

                //Send the email
                smtp.Send(email);

                return true;
            }

            catch (Exception ex)
            {
                Logger.Error("There was an error sending an email.");
                Logger.TraceException(ex.Message, ex);
            }

            return false;
        }
    }
}
