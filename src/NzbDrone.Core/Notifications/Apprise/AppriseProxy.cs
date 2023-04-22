using System;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Apprise
{
    public interface IAppriseProxy
    {
        void SendNotification(string title, string message, AppriseSettings settings);
        ValidationFailure Test(AppriseSettings settings);
    }

    public class AppriseProxy : IAppriseProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public AppriseProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(string title, string message, AppriseSettings settings)
        {
            var payload = new ApprisePayload
            {
                Title = title,
                Body = message,
                Type = (AppriseNotificationType)settings.NotificationType
            };

            var requestBuilder = new HttpRequestBuilder(settings.ServerUrl.TrimEnd('/', ' '))
                .Post()
                .Accept(HttpAccept.Json);

            if (settings.ConfigurationKey.IsNotNullOrWhiteSpace())
            {
                requestBuilder
                    .Resource("/notify/{configurationKey}")
                    .SetSegment("configurationKey", settings.ConfigurationKey);
            }
            else if (settings.StatelessUrls.IsNotNullOrWhiteSpace())
            {
                requestBuilder.Resource("/notify");

                payload.Urls = settings.StatelessUrls;
            }

            if (settings.Tags.Any())
            {
                payload.Tag = settings.Tags.Join(",");
            }

            if (settings.AuthUsername.IsNotNullOrWhiteSpace() || settings.AuthPassword.IsNotNullOrWhiteSpace())
            {
                requestBuilder.NetworkCredential = new BasicNetworkCredential(settings.AuthUsername, settings.AuthPassword);
            }

            var request = requestBuilder.Build();

            request.Headers.ContentType = "application/json";
            request.SetContent(payload.ToJson());

            try
            {
                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, "Unable to send message");
                throw new AppriseException("Unable to send Apprise notifications: {0}", ex, ex.Message);
            }
        }

        public ValidationFailure Test(AppriseSettings settings)
        {
            const string title = "Sonarr - Test Notification";
            const string body = "Success! You have properly configured your apprise notification settings.";

            try
            {
                SendNotification(title, body, settings);
            }
            catch (AppriseException ex) when (ex.InnerException is HttpException httpException)
            {
                if (httpException.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, $"HTTP Auth credentials are invalid: {0}", ex.Message);
                    return new ValidationFailure("AuthUsername", $"HTTP Auth credentials are invalid: {ex.Message}");
                }

                if (httpException.Response.Content.IsNotNullOrWhiteSpace())
                {
                    var error = Json.Deserialize<AppriseError>(httpException.Response.Content);

                    _logger.Error(ex, $"Unable to send test message. Response from API: {0}", error.Error);
                    return new ValidationFailure(string.Empty, $"Unable to send test message. Response from API: {error.Error}");
                }

                _logger.Error(ex, "Unable to send test message. Server connection failed: ({0}) {1}", httpException.Response.StatusCode, ex.Message);
                return new ValidationFailure("Url", $"Unable to connect to Apprise API. Server connection failed: ({httpException.Response.StatusCode}) {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message: {0}", ex.Message);
                return new ValidationFailure("Url", $"Unable to send test message: {ex.Message}");
            }

            return null;
        }
    }
}
