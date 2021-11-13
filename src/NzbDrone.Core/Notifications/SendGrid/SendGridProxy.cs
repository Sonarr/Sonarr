using System.Net;
using System.Net.Http;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.SendGrid
{
    public interface ISendGridProxy
    {
        void SendNotification(string title, string message, SendGridSettings settings);
    }

    public class SendGridProxy : ISendGridProxy
    {
        private readonly IHttpClient _httpClient;

        public SendGridProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SendNotification(string title, string message, SendGridSettings settings)
        {
            try
            {
                var request = BuildRequest(settings, "mail/send", HttpMethod.Post);

                var payload = new SendGridPayload
                {
                    From = new SendGridEmail
                    {
                        Email = settings.From
                    }
                };

                payload.Content.Add(new SendGridContent
                {
                    Type = "text/plain",
                    Value = message
                });

                var personalization = new SendGridPersonalization
                {
                    Subject = title,
                };

                foreach (var recipient in settings.Recipients)
                {
                    personalization.To.Add(new SendGridEmail
                    {
                        Email = recipient
                    });
                }

                payload.Personalizations.Add(personalization);

                request.SetContent(payload.ToJson());

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new SendGridException("Unauthorized - AuthToken is invalid");
                }

                throw new SendGridException("Unable to connect to SendGrid. Status Code: {0}", ex);
            }
        }

        private HttpRequest BuildRequest(SendGridSettings settings, string resource, HttpMethod method)
        {
            var request = new HttpRequestBuilder(settings.BaseUrl).Resource(resource)
                .SetHeader("Authorization", $"Bearer {settings.ApiKey}")
                .Build();

            request.Headers.ContentType = "application/json";
            request.Method = method;

            return request;
        }
    }
}
