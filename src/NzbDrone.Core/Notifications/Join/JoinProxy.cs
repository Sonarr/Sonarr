using System;
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
                throw;
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
            catch(JoinInvalidDeviceException ex)
            {
                _logger.Error(ex, "Unable to send test Join message. Invalid Device IDs supplied.");
                return new ValidationFailure("DeviceIds", "Device IDs appear invalid.");
            }
            catch (JoinException ex)
            {
                _logger.Error(ex, "Unable to send test Join message.");
                return new ValidationFailure("ApiKey", ex.Message);
            }
            catch(RestException ex)
            {
                _logger.Error(ex, "Unable to send test Join message. Server connection failed.");
                return new ValidationFailure("ApiKey", "Unable to connect to Join API. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test Join message. Unknown error.");
                return new ValidationFailure("ApiKey", ex.Message);
            }
        }

        private void SendNotification(string title, string message, RestRequest request, JoinSettings settings)
        {

            var client = RestClientFactory.BuildClient(URL);

            if (!string.IsNullOrEmpty(settings.DeviceIds))
            {
                request.AddParameter("deviceIds", settings.DeviceIds);
            }
            else
            {
                request.AddParameter("deviceId", "group.all");
            }
                
            request.AddParameter("apikey", settings.ApiKey);
            request.AddParameter("title", title);
            request.AddParameter("text", message);
            request.AddParameter("icon", "https://cdn.rawgit.com/Sonarr/Sonarr/develop/Logo/256.png"); // Use the Sonarr logo.

            var response = client.ExecuteAndValidate(request);
            var res = Json.Deserialize<JoinResponseModel>(response.Content);

            if (res.success) return;

            if (res.userAuthError)
            {
                throw new JoinAuthException("Authentication failed.");
            }

            if (res.errorMessage != null)
            {
                // Unfortunately hard coding this string here is the only way to determine that there aren't any devices to send to.
                // There isn't an enum or flag contained in the response that can be used instead.
                if (res.errorMessage.Equals("No devices to send to"))
                {
                    throw new JoinInvalidDeviceException(res.errorMessage);
                }
                // Oddly enough, rather than give us an "Invalid API key", the Join API seems to assume the key is valid,
                // but fails when doing a device lookup associated with that key.  
                // In our case we are using "deviceIds" rather than "deviceId" so when the singular form error shows up
                // we know the API key was the fault.
                else if (res.errorMessage.Equals("No device to send message to"))
                {
                    throw new JoinAuthException("Authentication failed.");
                }
                throw new JoinException(res.errorMessage);
            }

            throw new JoinException("Unknown error. Join message failed to send.");
        }
    }
}
