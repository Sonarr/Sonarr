using System;
using System.Net.Http;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Webhook
{
    public interface IWebhookProxy
    {
        void SendWebhook(WebhookPayload payload, WebhookSettings settings);
    }

    public class WebhookProxy : IWebhookProxy
    {
        private readonly IHttpClient _httpClient;

        public WebhookProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SendWebhook(WebhookPayload body, WebhookSettings settings)
        {
            try
            {
                var request = new HttpRequestBuilder(settings.Url)
                    .Accept(HttpAccept.Json)
                    .Build();

                request.Method = settings.Method switch
                {
                    (int)WebhookMethod.POST => HttpMethod.Post,
                    (int)WebhookMethod.PUT => HttpMethod.Put,
                    _ => throw new ArgumentOutOfRangeException($"Invalid Webhook method {settings.Method}")
                };

                request.Headers.ContentType = "application/json";
                request.SetContent(body.ToJson());

                if (settings.Username.IsNotNullOrWhiteSpace() || settings.Password.IsNotNullOrWhiteSpace())
                {
                    request.Credentials = new BasicNetworkCredential(settings.Username, settings.Password);
                }

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                throw new WebhookException("Unable to post to webhook: {0}", ex, ex.Message);
            }
        }
    }
}
