using System;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
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
        private const string URL = "https://joinjoaomgcd.appspot.com/_ah/api/messaging/v1/sendPush?";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public JoinProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(string title, string message, JoinSettings settings)
        {
            var method = HttpMethod.GET;

            try
            {
                SendNotification(title, message, method, settings);
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
            catch (JoinInvalidDeviceException ex)
            {
                _logger.Error(ex, "Unable to send test Join message. Invalid Device IDs supplied.");
                return new ValidationFailure("DeviceIds", "Device IDs appear invalid.");
            }
            catch (JoinException ex)
            {
                _logger.Error(ex, "Unable to send test Join message.");
                return new ValidationFailure("ApiKey", ex.Message);
            }
            catch (HttpException ex)
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

        private void SendNotification(string title, string message, HttpMethod method, JoinSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder(URL);

            if (settings.DeviceNames.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("deviceNames", settings.DeviceNames);
            }
            else if (settings.DeviceIds.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("deviceIds", settings.DeviceIds);
            }
            else
            {
                requestBuilder.AddQueryParam("deviceId", "group.all");
            }

            var request = requestBuilder.AddQueryParam("apikey", settings.ApiKey)
                          .AddQueryParam("title", title)
                          .AddQueryParam("text", message)
                          .AddQueryParam("icon", "https://cdn.rawgit.com/Sonarr/Sonarr/main/Logo/256.png") // Use the Sonarr logo.
                          .AddQueryParam("smallicon", "https://cdn.rawgit.com/Sonarr/Sonarr/main/Logo/96-Outline-White.png") // 96x96px with outline at 88x88px on a transparent background.
                          .AddQueryParam("priority", settings.Priority)
                          .Build();

            request.Method = method;

            var response = _httpClient.Execute(request);
            var res = Json.Deserialize<JoinResponseModel>(response.Content);

            if (res.success)
            {
                return;
            }

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
