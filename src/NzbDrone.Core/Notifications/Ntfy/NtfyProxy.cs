using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.Notifications.Ntfy
{
    public interface INtfyProxy
    {
        void SendNotification(string title, string message, NtfySettings settings);

        ValidationFailure Test(NtfySettings settings);
    }

    public class NtfyProxy : INtfyProxy
    {
        private const string DEFAULT_PUSH_URL = "https://ntfy.sh";

        private readonly IHttpClient _httpClient;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public NtfyProxy(IHttpClient httpClient, ILocalizationService localizationService, Logger logger)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _logger = logger;
        }

        public void SendNotification(string title, string message, NtfySettings settings)
        {
            var error = false;

            var serverUrl = settings.ServerUrl.IsNullOrWhiteSpace() ? DEFAULT_PUSH_URL : settings.ServerUrl;

            foreach (var topic in settings.Topics)
            {
                var request = BuildTopicRequest(serverUrl, topic);

                try
                {
                    SendNotification(title, message, request, settings);
                }
                catch (NtfyException ex)
                {
                    _logger.Error(ex, "Unable to send test message to {0}", topic);
                    error = true;
                }
            }

            if (error)
            {
                throw new NtfyException("Unable to send Ntfy notifications to all topics");
            }
        }

        private HttpRequestBuilder BuildTopicRequest(string serverUrl, string topic)
        {
            var trimServerUrl = serverUrl.TrimEnd('/');

            var requestBuilder = new HttpRequestBuilder($"{trimServerUrl}/{topic}").Post();

            return requestBuilder;
        }

        public ValidationFailure Test(NtfySettings settings)
        {
            try
            {
                const string title = "Sonarr - Test Notification";

                const string body = "This is a test message from Sonarr";

                SendNotification(title, body, settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    if (!settings.AccessToken.IsNullOrWhiteSpace())
                    {
                        _logger.Error(ex, "Invalid token");
                        return new ValidationFailure("AccessToken", _localizationService.GetLocalizedString("NotificationsValidationInvalidAccessToken"));
                    }

                    if (!settings.UserName.IsNullOrWhiteSpace() && !settings.Password.IsNullOrWhiteSpace())
                    {
                        _logger.Error(ex, "Invalid username or password");
                        return new ValidationFailure("UserName", _localizationService.GetLocalizedString("NotificationsValidationInvalidUsernamePassword"));
                    }

                    _logger.Error(ex, "Authorization is required");
                    return new ValidationFailure("AccessToken", _localizationService.GetLocalizedString("NotificationsNtfyValidationAuthorizationRequired"));
                }

                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("ServerUrl", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure(string.Empty,  _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private void SendNotification(string title, string message, HttpRequestBuilder requestBuilder, NtfySettings settings)
        {
            try
            {
                requestBuilder.Headers.Add("X-Title", title);
                requestBuilder.Headers.Add("X-Message", message);
                requestBuilder.Headers.Add("X-Priority", settings.Priority.ToString());

                if (settings.Tags.Any())
                {
                    requestBuilder.Headers.Add("X-Tags", settings.Tags.Join(","));
                }

                if (!settings.ClickUrl.IsNullOrWhiteSpace())
                {
                    requestBuilder.Headers.Add("X-Click", settings.ClickUrl);
                }

                if (!settings.AccessToken.IsNullOrWhiteSpace())
                {
                    requestBuilder.Headers.Set("Authorization", $"Bearer {settings.AccessToken}");
                }
                else if (!settings.UserName.IsNullOrWhiteSpace() && !settings.Password.IsNullOrWhiteSpace())
                {
                    requestBuilder.NetworkCredential = new BasicNetworkCredential(settings.UserName, settings.Password);
                }

                var request = requestBuilder.Build();

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    _logger.Error(ex, "Authorization is required");
                    throw;
                }

                throw new NtfyException("Unable to send text message: {0}", ex, ex.Message);
            }
        }
    }
}
