using System;
using System.Linq;
using System.Net;

using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

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

        private readonly Logger _logger;

        public NtfyProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(string title, string message, NtfySettings settings)
        {
            var error = false;

            var serverUrl = settings.ServerUrl.IsNullOrWhiteSpace() ? NtfyProxy.DEFAULT_PUSH_URL : settings.ServerUrl;

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
                const string title = "Radarr - Test Notification";

                const string body = "This is a test message from Radarr";

                SendNotification(title, body, settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized || ex.Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.Error(ex, "Authorization is required");
                    return new ValidationFailure("UserName", "Authorization is required");
                }

                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("ServerUrl", "Unable to send test message");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test message");
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

                var request = requestBuilder.Build();

                if (!settings.UserName.IsNullOrWhiteSpace() && !settings.Password.IsNullOrWhiteSpace())
                {
                    request.Credentials = new BasicNetworkCredential(settings.UserName, settings.Password);
                }

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized || ex.Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.Error(ex, "Authorization is required");
                    throw;
                }

                throw new NtfyException("Unable to send text message: {0}", ex, ex.Message);
            }
        }
    }
}
