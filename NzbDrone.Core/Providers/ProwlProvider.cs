using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Prowlin;

namespace NzbDrone.Core.Providers
{
    public class ProwlProvider
    {
        private readonly Logger Logger = LogManager.GetLogger("ProwlProvider");

        public ProwlProvider()
        {

        }

        public virtual bool Verify(string apiKey)
        {
            try
            {
                var verificationRequest = new Verification();
                verificationRequest.ApiKey = apiKey;

                var client = new ProwlClient();

                Logger.Trace("Verifying API Key: {0}", apiKey);

                var verificationResult = client.SendVerification(verificationRequest);
                if (String.IsNullOrWhiteSpace(verificationResult.ErrorMessage) && verificationResult.ResultCode == "200")
                    return true;
            }

            catch(Exception ex)
            {
                Logger.TraceException(ex.Message, ex);
                Logger.Warn("Invalid API Key: {0}", apiKey);
            }

            return false;
        }

        public virtual bool SendNotification(string title, string message, string apiKeys, NotificationPriority priority = NotificationPriority.Normal, string url = null)
        {
            try
            {
                var notification = new Notification
                                   {
                                           Application = "NzbDrone",
                                           Description = message,
                                           Event = title,
                                           Priority = priority,
                                           Url = url
                                   };

                foreach (var apiKey in apiKeys.Split(','))
                    notification.AddApiKey(apiKey.Trim());

                var client = new ProwlClient();

                Logger.Trace("Sending Prowl Notification");

                var notificationResult = client.SendNotification(notification);

                if (String.IsNullOrWhiteSpace(notificationResult.ErrorMessage))
                    return true;
            }

            catch(Exception ex)
            {
                Logger.TraceException(ex.Message, ex);
                Logger.Warn("Invalid API Key(s): {0}", apiKeys);
            }

            return false;
        }

        public virtual void TestNotification(string apiKeys)
        {
            const string title = "Test Notification";
            const string message = "This is a test message from NzbDrone";

            SendNotification(title, message, apiKeys);
        }
    }
}
