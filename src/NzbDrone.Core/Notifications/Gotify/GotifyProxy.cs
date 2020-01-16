using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.Gotify
{
    public interface IGotifyProxy
    {
        void SendNotification(string title, string message, GotifySettings settings);
    }

    public class GotifyProxy : IGotifyProxy
    {
        private readonly IRestClientFactory _restClientFactory;

        public GotifyProxy(IRestClientFactory restClientFactory)
        {
            _restClientFactory = restClientFactory;
        }

        public void SendNotification(string title, string message, GotifySettings settings)
        {
            var client = _restClientFactory.BuildClient(settings.Server);
            var request = new RestRequest("message", Method.POST);

            request.AddQueryParameter("token", settings.AppToken);
            request.AddParameter("title", title);
            request.AddParameter("message", message);
            request.AddParameter("priority", settings.Priority);

            client.ExecuteAndValidate(request);
        }
    }
}
