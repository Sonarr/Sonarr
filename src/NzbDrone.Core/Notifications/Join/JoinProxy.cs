using System;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using RestSharp;
using NzbDrone.Core.Rest;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Join
{
    public interface IJoinProxy
    {
        void SendNotification(string title, string message, JoinSettings settings);
        ValidationFailure Test(JoinSettings settings);
    }

    public class JoinProxy : IJoinProxy
    {
        private readonly Logger _logger;
        private const string URL = "https://joinjoaomgcd.appspot.com/_ah/api/messaging/v1/sendPush?";

        public JoinProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, JoinSettings settings)
        {
            var request = new RestRequest(Method.GET);

            try
            {
                SendNotification(title, message, request, settings);
            }
            catch (JoinException ex)
            {
                _logger.Error(ex, "Unable to send Join message.");
                throw new JoinException("Unable to send Join notifications. " + ex.Message);
            }
        }

        public ValidationFailure Test(JoinSettings settings)
        {
            const string title = "Test Notification";
            const string body = "This is a test message from Sonarr.";

            try
            {
                SendNotification(title, body, settings);
                return null;
            }
            catch (JoinException ex)
            {
                _logger.Error(ex, "Unable to send test Join message.");
                return new ValidationFailure("APIKey", ex.Message);
            }
        }

        private void SendNotification(string title, string message, RestRequest request, JoinSettings settings)
        {
            try
            {
                var client = RestClientFactory.BuildClient(URL);

                request.AddParameter("deviceId", "group.all");
                request.AddParameter("apikey", settings.APIKey);
                request.AddParameter("title", title);
                request.AddParameter("text", message);
                request.AddParameter("icon", "https://cdn.rawgit.com/Sonarr/Sonarr/develop/Logo/256.png"); // Use the Sonarr logo.

                var response = client.ExecuteAndValidate(request);
                var res = Json.Deserialize<JoinResponseModel>(response.Content);

                if (res.success) return;

                if (res.errorMessage != null)
                {
                    throw new JoinException(res.errorMessage);
                }

                if (res.userAuthError)
                {
                    throw new JoinAuthException("Authentication failed.");
                }

                throw new JoinException("Unknown error. Join message failed to send.");      
            }
            catch(Exception e)
            {
                throw new JoinException(e.Message, e);
            }
        }
    }
}
