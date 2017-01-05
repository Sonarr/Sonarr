using System;
using FluentValidation.Results;
using NLog;
using Prowlin;

namespace NzbDrone.Core.Notifications.Prowl
{
    public interface IProwlService
    {
        void SendNotification(string title, string message, string apiKey, NotificationPriority priority = NotificationPriority.Normal, string url = null);
        ValidationFailure Test(ProwlSettings settings);
    }

    public class ProwlService : IProwlService
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
                                       Application = "Sonarr",
                                       Description = message,
                                       Event = title,
                                       Priority = priority,
                                       Url = url
                                   };

                notification.AddApiKey(apiKey.Trim());

                var client = new ProwlClient();

                _logger.Debug("Sending Prowl Notification");

                var notificationResult = client.SendNotification(notification);

                if (!string.IsNullOrWhiteSpace(notificationResult.ErrorMessage))
                {
                    throw new InvalidApiKeyException("API Key: " + apiKey + " is invalid");
                }
            }

            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
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

                _logger.Debug("Verifying API Key: {0}", apiKey);

                var verificationResult = client.SendVerification(verificationRequest);
                if (!string.IsNullOrWhiteSpace(verificationResult.ErrorMessage) &&
                    verificationResult.ResultCode != "200")
                {
                    throw new InvalidApiKeyException("API Key: " + apiKey + " is invalid");
                }
            }

            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
                _logger.Warn("Invalid API Key: {0}", apiKey);
                throw new InvalidApiKeyException("API Key: " + apiKey + " is invalid");
            }
        }

        public ValidationFailure Test(ProwlSettings settings)
        {
            try
            {
                Verify(settings.ApiKey);

                const string title = "Test Notification";
                const string body = "This is a test message from Sonarr";

                SendNotification(title, body, settings.ApiKey);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("ApiKey", "Unable to send test message");
            }

            return null;
        }
    }
}
