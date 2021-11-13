using System.Net;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Notifications.Mailgun
{
    public interface IMailgunProxy
    {
        void SendNotification(string tittle, string message, MailgunSettings settings);
    }

    public class MailgunProxy : IMailgunProxy
    {
        private const string BaseUrlEu = "https://api.eu.mailgun.net/v3";
        private const string BaseUrlUs = "https://api.mailgun.net/v3";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MailgunProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(string title, string message, MailgunSettings settings)
        {
            try
            {
                var request = BuildRequest(settings, $"{settings.SenderDomain}/messages", HttpMethod.Post, title, message).Build();
                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new MailgunException("Unauthorised - ApiKey is invalid");
                }

                throw new MailgunException("Unable to connect to Mailgun. Status code: {0}", ex);
            }
        }

        private HttpRequestBuilder BuildRequest(MailgunSettings settings, string resource, HttpMethod method, string messageSubject, string messageBody)
        {
            var loginCredentials = new NetworkCredential("api", settings.ApiKey);
            var url = settings.UseEuEndpoint ? BaseUrlEu : BaseUrlUs;
            var requestBuilder = new HttpRequestBuilder(url).Resource(resource);

            requestBuilder.Method = method;
            requestBuilder.NetworkCredential = loginCredentials;

            requestBuilder.AddFormParameter("from", $"{settings.From}");

            foreach (var recipient in settings.Recipients)
            {
                requestBuilder.AddFormParameter("to", $"{recipient}");
            }

            requestBuilder.AddFormParameter("subject", $"{messageSubject}");
            requestBuilder.AddFormParameter("text", $"{messageBody}");

            return requestBuilder;
        }
    }
}
