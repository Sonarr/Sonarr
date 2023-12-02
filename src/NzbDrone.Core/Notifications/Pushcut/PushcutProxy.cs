using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.Notifications.Pushcut
{
    public interface IPushcutProxy
    {
        void SendNotification(string title, string message, PushcutSettings settings);
        ValidationFailure Test(PushcutSettings settings);
    }

    public class PushcutProxy : IPushcutProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public PushcutProxy(IHttpClient httpClient, ILocalizationService localizationService, Logger logger)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _logger = logger;
        }

        public void SendNotification(string title, string message, PushcutSettings settings)
        {
            var request = new HttpRequestBuilder("https://api.pushcut.io/v1/notifications/{notificationName}")
                .SetSegment("notificationName", settings?.NotificationName)
                .SetHeader("API-Key", settings?.ApiKey)
                .Accept(HttpAccept.Json)
                .Build();
            var payload = new PushcutPayload
            {
                Title = title,
                Text = message,
                IsTimeSensitive = settings?.TimeSensitive
            };

            request.Method = HttpMethod.Post;
            request.Headers.ContentType = "application/json";
            request.SetContent(payload.ToJson());

            try
            {
                _httpClient.Execute(request);
            }
            catch (HttpException exception)
            {
                _logger.Error(exception, "Unable to send Pushcut notification: {0}", exception.Message);
                throw new PushcutException("Unable to send Pushcut notification: {0}", exception.Message, exception);
            }
        }

        public ValidationFailure Test(PushcutSettings settings)
        {
            try
            {
                const string title = "Sonarr Test Title";
                const string message = "Success! You have properly configured your Pushcut notification settings.";
                SendNotification(title, message, settings);
            }
            catch (PushcutException pushcutException) when (pushcutException.InnerException is HttpException httpException)
            {
                if (httpException.Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.Error(pushcutException, "API Key is invalid: {0}", pushcutException.Message);
                    return new ValidationFailure("API Key", _localizationService.GetLocalizedString("NotificationsValidationInvalidApiKeyExceptionMessage", new Dictionary<string, object> { { "exceptionMessage", pushcutException.Message } }));
                }

                if (httpException.Response.Content.IsNotNullOrWhiteSpace())
                {
                    var response = Json.Deserialize<PushcutResponse>(httpException.Response.Content);

                    _logger.Error(pushcutException, "Unable to send test notification. Response from Pushcut: {0}", response.Error);
                    return new ValidationFailure("Url", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessageApiResponse", new Dictionary<string, object> { { "error", response.Error } }));
                }

                _logger.Error(pushcutException, "Unable to connect to Pushcut API. Server connection failed: ({0}) {1}", httpException.Response.StatusCode, pushcutException.Message);
                return new ValidationFailure("Host", _localizationService.GetLocalizedString("NotificationsValidationUnableToConnectToApi", new Dictionary<string, object> { { "responseCode", httpException.Response.StatusCode }, { "exceptionMessage", pushcutException.Message } }));
            }

            return null;
        }
    }
}
