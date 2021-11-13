using System.Net.Http;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Slack.Payloads;

namespace NzbDrone.Core.Notifications.Slack
{
    public interface ISlackProxy
    {
        void SendPayload(SlackPayload payload, SlackSettings settings);
    }

    public class SlackProxy : ISlackProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public SlackProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendPayload(SlackPayload payload, SlackSettings settings)
        {
            try
            {
                var request = new HttpRequestBuilder(settings.WebHookUrl)
                    .Accept(HttpAccept.Json)
                    .Build();

                request.Method = HttpMethod.Post;
                request.Headers.ContentType = "application/json";
                request.SetContent(payload.ToJson());

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, "Unable to post payload {0}", payload);
                throw new SlackExeption("Unable to post payload", ex);
            }
        }
    }
}
