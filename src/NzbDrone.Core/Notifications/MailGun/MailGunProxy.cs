using System.Linq;
using System.Net;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;

namespace NzbDrone.Core.Notifications.MailGun
{
    public interface IMailGunProxy
    {
        void SendNotification(string tittle, string message, MailGunSettings settings);
    }
    
    public class MailGunProxy : IMailGunProxy
    {
        private readonly IHttpClient _httpClient;

        public MailGunProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SendNotification(string title, string message, MailGunSettings settings)
        {
            try
            {
                var request = BuildRequest(settings, $"{settings.Domain}/messages", HttpMethod.POST);

                var payload = new MailGunPayload();

                request.SetContent(payload.ToJson());
                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new MailGunException("Unauthorised - ApiKey is invalid");
                }
                
                throw new MailGunException("Unable to connect to MailGun. Status code: {0}", ex);
            }
        }


        private HttpRequest BuildRequest(MailGunSettings settings, string resource, HttpMethod method)
        {
            var creds = new NetworkCredential("api", settings.ApiKey);
            
            
            if (settings.IsEu)
            {
                // TODO: Needs a way to use login credentials, like username:pass.
                // Reference: https://documentation.mailgun.com/en/latest/api-sending.html#examples
                var request = new HttpRequestBuilder(settings.BaseUrlEu).Resource(resource)
                    .Build();

                request.Method = method;
                
                return request;
            }
            else
            {
                // TODO: Needs a way to use login credentials, like username:pass.
                // Reference: https://documentation.mailgun.com/en/latest/api-sending.html#examples
                var request = new HttpRequestBuilder(settings.BaseUrlUs).Resource(resource).Build();

                request.Method = method;
                
                return request;
            }
        }
    }
}
