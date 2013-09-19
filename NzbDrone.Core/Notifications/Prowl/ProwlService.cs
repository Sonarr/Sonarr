using System;
using NLog;
using NzbDrone.Core.Messaging.Commands;
using Prowlin;

namespace NzbDrone.Core.Notifications.Prowl
{
    public interface IProwlService
    {
        void SendNotification(string title, string message, string apiKey, NotificationPriority priority = NotificationPriority.Normal, string url = null);
    }

    public class ProwlService : IProwlService, IExecute<TestProwlCommand>
    {
        private readonly Logger _logger;

        public ProwlService(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, string apiKey, NotificationPriority priority = NotificationPriority.Normal, string url = null)
        {
            try
            {
                var notification = new Prowlin.Notification
                                   {
                                       Application = "NzbDrone",
                                       Description = message,
                                       Event = title,
                                       Priority = priority,
                                       Url = url
                                   };

                notification.AddApiKey(apiKey.Trim());

                var client = new ProwlClient();

                _logger.Trace("Sending Prowl Notification");

                var notificationResult = client.SendNotification(notification);

                if (!String.IsNullOrWhiteSpace(notificationResult.ErrorMessage))
                {
                    throw new InvalidApiKeyException("API Key: " + apiKey + " is invalid");
                }
            }

            catch (Exception ex)
            {
                _logger.TraceException(ex.Message, ex);
                _logger.Warn("Invalid API Key: {0}", apiKey);
            }
        }

        public void Verify(string apiKey)
        {
            try
            {
                var verificationRequest = new Verification();
                verificationRequest.ApiKey = apiKey;

                var client = new ProwlClient();

                _logger.Trace("Verifying API Key: {0}", apiKey);

                var verificationResult = client.SendVerification(verificationRequest);
                if (!String.IsNullOrWhiteSpace(verificationResult.ErrorMessage) &&
                    verificationResult.ResultCode != "200")
                {
                    throw new InvalidApiKeyException("API Key: " + apiKey + " is invalid");
                }
            }

            catch (Exception ex)
            {
                _logger.TraceException(ex.Message, ex);
                _logger.Warn("Invalid API Key: {0}", apiKey);
                throw new InvalidApiKeyException("API Key: " + apiKey + " is invalid");
            }
        }

        public void Execute(TestProwlCommand message)
        {
            Verify(message.ApiKey);

            const string title = "Test Notification";
            const string body = "This is a test message from NzbDrone";

            SendNotification(title, body, message.ApiKey);
        }
    }
}
