using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.Boxcar
{
    public interface IBoxcarProxy
    {
        void SendNotification(string title, string message, BoxcarSettings settings);
        ValidationFailure Test(BoxcarSettings settings);
    }

    public class BoxcarProxy : IBoxcarProxy
    {
        private readonly Logger _logger;
        private const string URL = "https://new.boxcar.io/api/notifications";

        public BoxcarProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, BoxcarSettings settings)
        {
            var request = new RestRequest(Method.POST);

            try
            {
                SendNotification(title, message, request, settings);
            }
            catch (BoxcarException ex)
            {
                _logger.Error(ex, "Unable to send message");
                throw new BoxcarException("Unable to send Boxcar notifications");
            }
        }

        public ValidationFailure Test(BoxcarSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Sonarr";

                SendNotification(title, body, settings);
                return null;
            }
            catch (RestException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access Token is invalid");
                    return new ValidationFailure("Token", "Access Token is invalid");
                }

                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Token", "Unable to send test message");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test message");
            }
        }

        private void SendNotification(string title, string message, RestRequest request, BoxcarSettings settings)
        {
            try
            {
                var client = RestClientFactory.BuildClient(URL);

                request.AddParameter("user_credentials", settings.Token);
                request.AddParameter("notification[title]", title);
                request.AddParameter("notification[long_message]", message);
                request.AddParameter("notification[source_name]", "Sonarr");
                request.AddParameter("notification[icon_url]", "https://raw.githubusercontent.com/Sonarr/Sonarr/7818f0c59b787312f0bcbc5c0eafc3c9dd7e5451/Logo/64.png");

                client.ExecuteAndValidate(request);
            }
            catch (RestException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access Token is invalid");
                    throw;
                }

                throw new BoxcarException("Unable to send text message: " + ex.Message, ex);
            }
        }
    }
}
