using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowserProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MediaBrowserProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void Notify(MediaBrowserSettings settings, string title, string message)
        {
            var path = "/Notifications/Admin";
            var request = BuildRequest(path, settings);
            request.Headers.ContentType = "application/json";

            request.SetContent(new
                           {
                               Name = title,
                               Description = message,
                               ImageUrl = "https://raw.github.com/NzbDrone/NzbDrone/develop/Logo/64.png"
                           }.ToJson());

            ProcessRequest(request, settings);
        }

        public void Update(MediaBrowserSettings settings, int tvdbId)
        {
            var path = string.Format("/Library/Series/Updated?tvdbid={0}", tvdbId);            
            var request = BuildRequest(path, settings);
            request.Headers.Add("Content-Length", "0");

            ProcessRequest(request, settings);
        }

        private string ProcessRequest(HttpRequest request, MediaBrowserSettings settings)
        {
            request.Headers.Add("X-MediaBrowser-Token", settings.ApiKey);

            var response = _httpClient.Post(request);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }

        private HttpRequest BuildRequest(string path, MediaBrowserSettings settings)
        {
            var url = string.Format(@"http://{0}/mediabrowser", settings.Address);
            
            return new HttpRequestBuilder(url).Resource(path).Build();
        }

        private void CheckForError(HttpResponse response)
        {
            _logger.Debug("Looking for error in response: {0}", response);

            //TODO: actually check for the error
        }
    }
}
