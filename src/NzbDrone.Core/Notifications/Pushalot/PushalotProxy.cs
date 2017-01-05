using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Notifications.Pushalot
{
    public interface IPushalotProxy
    {
        void SendNotification(string title, string message, PushalotSettings settings);
        ValidationFailure Test(PushalotSettings settings);
    }

    public class PushalotProxy : IPushalotProxy
    {
        private readonly Logger _logger;
        private const string URL = "https://pushalot.com/api/sendmessage";

        public PushalotProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, PushalotSettings settings)
        {
            var client = RestClientFactory.BuildClient(URL);
            var request = BuildRequest();

            request.AddParameter("Source", "Sonarr");

            if (settings.Image)
            {
                request.AddParameter("Image", "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/128.png");                
            }

            request.AddParameter("Title", title);
            request.AddParameter("Body", message);
            request.AddParameter("AuthorizationToken", settings.AuthToken);

            if ((PushalotPriority)settings.Priority == PushalotPriority.Important)
            {
                request.AddParameter("IsImportant", true);
            }

            if ((PushalotPriority)settings.Priority == PushalotPriority.Silent)
            {
                request.AddParameter("IsSilent", true);
            }

            client.ExecuteAndValidate(request);
        }

        public RestRequest BuildRequest()
        {
            var request = new RestRequest(Method.POST);

            return request;
        }

        public ValidationFailure Test(PushalotSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Sonarr";

                SendNotification(title, body, settings);
            }
            catch (RestException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Authentication Token is invalid");
                    return new ValidationFailure("AuthToken", "Authentication Token is invalid");
                }

                if (ex.Response.StatusCode == HttpStatusCode.NotAcceptable)
                {
                    _logger.Error(ex, "Message limit reached");
                    return new ValidationFailure("AuthToken", "Message limit reached");
                }

                if (ex.Response.StatusCode == HttpStatusCode.Gone)
                {
                    _logger.Error(ex, "Authorization Token is no longer valid");
                    return new ValidationFailure("AuthToken", "Authorization Token is no longer valid, please use a new one.");
                }

                var response = Json.Deserialize<PushalotResponse>(ex.Response.Content);

                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("AuthToken", response.Description);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test message");
            }

            return null;
        }
    }
}
