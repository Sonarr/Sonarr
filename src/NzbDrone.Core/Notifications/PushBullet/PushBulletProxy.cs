using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public interface IPushBulletProxy
    {
        void SendNotification(string title, string message, string apiKey, string deviceId);
        ValidationFailure Test(PushBulletSettings settings);
    }

    public class PushBulletProxy : IPushBulletProxy
    {
        private readonly Logger _logger;
        private const string URL = "https://api.pushbullet.com/api/pushes";

        public PushBulletProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, string apiKey, string deviceId)
        {
            var client = RestClientFactory.BuildClient(URL);
            var request = BuildRequest(deviceId); 
            
            request.AddParameter("type", "note");
            request.AddParameter("title", title);
            request.AddParameter("body", message);

            client.Authenticator = new HttpBasicAuthenticator(apiKey, String.Empty);
            client.ExecuteAndValidate(request);
        }

        public RestRequest BuildRequest(string deviceId)
        {
            var request = new RestRequest(Method.POST);
            long integerId;

            if (Int64.TryParse(deviceId, out integerId))
            {
                request.AddParameter("device_id", integerId);
            }

            else
            {
                request.AddParameter("device_iden", deviceId);
            }

            return request;
        }

        public ValidationFailure Test(PushBulletSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from NzbDrone";

                SendNotification(title, body, settings.ApiKey, settings.DeviceId);
            }
            catch (RestException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.ErrorException("API Key is invalid: " + ex.Message, ex);
                    return new ValidationFailure("ApiKey", "API Key is invalid");
                }

                _logger.ErrorException("Unable to send test message: " + ex.Message, ex);
                return new ValidationFailure("ApiKey", "Unable to send test message");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to send test message: " + ex.Message, ex);
                return new ValidationFailure("", "Unable to send test message");
            }

            return null;
        }
    }
}
