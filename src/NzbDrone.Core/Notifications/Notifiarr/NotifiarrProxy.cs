using System.Net.Http;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Webhook;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public interface INotifiarrProxy
    {
        void SendNotification(WebhookPayload payload, NotifiarrSettings settings);
    }

    public class NotifiarrProxy : INotifiarrProxy
    {
        private const string URL = "https://notifiarr.com";
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public NotifiarrProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(WebhookPayload payload, NotifiarrSettings settings)
        {
            ProcessNotification(payload, settings);
        }

        private void ProcessNotification(WebhookPayload payload, NotifiarrSettings settings)
        {
            try
            {
                var request = new HttpRequestBuilder(URL + "/api/v1/notification/sonarr")
                    .Accept(HttpAccept.Json)
                    .SetHeader("X-API-Key", settings.ApiKey)
                    .Build();

                request.Method = HttpMethod.Post;

                request.Headers.ContentType = "application/json";
                request.SetContent(payload.ToJson());

                _httpClient.Post(request);
            }
            catch (HttpException ex)
            {
                var responseCode = ex.Response.StatusCode;
                switch ((int)responseCode)
                {
                    case 401:
                        _logger.Error("HTTP 401 - API key is invalid");
                        throw new NotifiarrException("API key is invalid");
                    case 400:
                        _logger.Error("HTTP 400 - Unable to send notification. Ensure Sonarr Integration is enabled & assigned a channel on Notifiarr");
                        throw new NotifiarrException("Unable to send notification. Ensure Sonarr Integration is enabled & assigned a channel on Notifiarr");
                    case 502:
                    case 503:
                    case 504:
                        _logger.Error("Unable to send notification. Service Unavailable");
                        throw new NotifiarrException("Unable to send notification. Service Unavailable", ex);
                    case 520:
                    case 521:
                    case 522:
                    case 523:
                    case 524:
                        throw new NotifiarrException("Cloudflare Related HTTP Error - Unable to send notification", ex);
                    default:
                        _logger.Error(ex, "Unknown HTTP Error - Unable to send notification");
                        throw new NotifiarrException("Unknown HTTP Error - Unable to send notification", ex);
                }
            }
        }
    }
}
