using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Notifications.Boxcar
{
    public interface IBoxcarProxy
    {
        void SendNotification(string title, string message, BoxcarSettings settings);
        ValidationFailure Test(BoxcarSettings settings);
    }

    public class BoxcarProxy : IBoxcarProxy
    {
        private const string URL = "https://new.boxcar.io/api/notifications";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public BoxcarProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(string title, string message, BoxcarSettings settings)
        {
            try
            {
                ProcessNotification(title, message, settings);
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
            catch (HttpException ex)
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

        private void ProcessNotification(string title, string message, BoxcarSettings settings)
        {
            try
            {
                var requestBuilder = new HttpRequestBuilder(URL).Post();

                var request = requestBuilder.AddFormParameter("user_credentials", settings.Token)
                    .AddFormParameter("notification[title]", title)
                    .AddFormParameter("notification[long_message]", message)
                    .AddFormParameter("notification[source_name]", BuildInfo.AppName)
                    .AddFormParameter("notification[icon_url]", "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/64.png")
                    .Build();

                _httpClient.Post(request);
            }
            catch (HttpException ex)
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
