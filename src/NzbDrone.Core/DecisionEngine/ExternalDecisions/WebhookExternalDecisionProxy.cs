using System;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public interface IWebhookExternalDecisionProxy
    {
        ExternalRejectionResponse SendRejectionRequest(ExternalRejectionRequest payload, WebhookExternalDecisionSettings settings);
        ExternalPrioritizationResponse SendPrioritizationRequest(ExternalPrioritizationRequest payload, WebhookExternalDecisionSettings settings);
    }

    public class WebhookExternalDecisionProxy : IWebhookExternalDecisionProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public WebhookExternalDecisionProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public ExternalRejectionResponse SendRejectionRequest(ExternalRejectionRequest payload, WebhookExternalDecisionSettings settings)
        {
            try
            {
                var request = new HttpRequestBuilder(settings.Url)
                    .Accept(HttpAccept.Json)
                    .Build();

                request.Headers.ContentType = "application/json";
                request.SetContent(Json.ToJson(payload));
                request.RequestTimeout = TimeSpan.FromSeconds(settings.Timeout);

                if (settings.ApiKey.IsNotNullOrWhiteSpace())
                {
                    request.Headers.Add("X-Api-Key", settings.ApiKey);
                }

                var response = _httpClient.Post(request);

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                {
                    if (response.Content.IsNullOrWhiteSpace())
                    {
                        return new ExternalRejectionResponse { Approved = true };
                    }

                    return Json.Deserialize<ExternalRejectionResponse>(response.Content);
                }

                _logger.Warn("External decision returned unexpected status code {0}, treating as approved (fail-open).", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Error communicating with external rejection decision at {0}, treating as approved (fail-open).", settings.Url);
            }

            return new ExternalRejectionResponse { Approved = true };
        }

        public ExternalPrioritizationResponse SendPrioritizationRequest(ExternalPrioritizationRequest payload, WebhookExternalDecisionSettings settings)
        {
            try
            {
                var request = new HttpRequestBuilder(settings.Url)
                    .Accept(HttpAccept.Json)
                    .Build();

                request.Headers.ContentType = "application/json";
                request.SetContent(Json.ToJson(payload));
                request.RequestTimeout = TimeSpan.FromSeconds(settings.Timeout);

                if (settings.ApiKey.IsNotNullOrWhiteSpace())
                {
                    request.Headers.Add("X-Api-Key", settings.ApiKey);
                }

                var response = _httpClient.Post(request);

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                {
                    if (response.Content.IsNullOrWhiteSpace())
                    {
                        return null;
                    }

                    return Json.Deserialize<ExternalPrioritizationResponse>(response.Content);
                }

                _logger.Warn("External prioritization decision returned unexpected status code {0}, keeping default order (fail-open).", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Error communicating with external prioritization decision at {0}, keeping default order (fail-open).", settings.Url);
            }

            return null;
        }
    }
}
