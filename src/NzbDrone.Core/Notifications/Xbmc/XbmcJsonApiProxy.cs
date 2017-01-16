using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Rest;
using RestSharp;
using RestSharp.Authenticators;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public interface IXbmcJsonApiProxy
    {
        string GetJsonVersion(XbmcSettings settings);
        void Notify(XbmcSettings settings, string title, string message);
        string UpdateLibrary(XbmcSettings settings, string path);
        void CleanLibrary(XbmcSettings settings);
        List<ActivePlayer> GetActivePlayers(XbmcSettings settings);
        List<TvShow> GetSeries(XbmcSettings settings);
    }

    public class XbmcJsonApiProxy : IXbmcJsonApiProxy
    {
        private readonly Logger _logger;

        public XbmcJsonApiProxy(Logger logger)
        {
            _logger = logger;
        }

        public string GetJsonVersion(XbmcSettings settings)
        {
            var request = new RestRequest();
            return ProcessRequest(request, settings, "JSONRPC.Version");
        }

        public void Notify(XbmcSettings settings, string title, string message)
        {
            var request = new RestRequest();

            var parameters = new Dictionary<string, object>();
            parameters.Add("title", title);
            parameters.Add("message", message);
            parameters.Add("image", "https://raw.github.com/Sonarr/Sonarr/develop/Logo/64.png");
            parameters.Add("displaytime", settings.DisplayTime * 1000);

            ProcessRequest(request, settings, "GUI.ShowNotification", parameters);
        }

        public string UpdateLibrary(XbmcSettings settings, string path)
        {
            var request = new RestRequest();
            var parameters = new Dictionary<string, object>();
            parameters.Add("directory", path);

            if (path.IsNullOrWhiteSpace())
            {
                parameters = null;
            }

            var response = ProcessRequest(request, settings, "VideoLibrary.Scan", parameters);

            return Json.Deserialize<XbmcJsonResult<string>>(response).Result;
        }

        public void CleanLibrary(XbmcSettings settings)
        {
            var request = new RestRequest();

            ProcessRequest(request, settings, "VideoLibrary.Clean");
        }

        public List<ActivePlayer> GetActivePlayers(XbmcSettings settings)
        {
            var request = new RestRequest();

            var response = ProcessRequest(request, settings, "Player.GetActivePlayers");

            return Json.Deserialize<ActivePlayersEdenResult>(response).Result;
        }

        public List<TvShow> GetSeries(XbmcSettings settings)
        {
            var request = new RestRequest();
            var parameters = new Dictionary<string, object>();
            parameters.Add("properties", new[] { "file", "imdbnumber" });

            var response = ProcessRequest(request, settings, "VideoLibrary.GetTvShows", parameters);

            return Json.Deserialize<TvShowResponse>(response).Result.TvShows;
        }

        private string ProcessRequest(IRestRequest request, XbmcSettings settings, string method, Dictionary<string, object> parameters = null)
        {
            var client = BuildClient(settings);

            request.Method = Method.POST;
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = new JsonNetSerializer();
            request.AddBody(new { jsonrpc = "2.0", method = method, id = 10, @params = parameters });

            var response = client.ExecuteAndValidate(request);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }

        private IRestClient BuildClient(XbmcSettings settings)
        {
            var url = string.Format(@"http://{0}/jsonrpc", settings.Address);
            var client = RestClientFactory.BuildClient(url);

            if (!settings.Username.IsNullOrWhiteSpace())
            {
                client.Authenticator = new HttpBasicAuthenticator(settings.Username, settings.Password);
            }

            return client;
        }

        private void CheckForError(IRestResponse response)
        {
            _logger.Debug("Looking for error in response: {0}", response);

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                throw new XbmcJsonException("Invalid response from XBMC, the response is not valid JSON");
            }

            if (response.Content.StartsWith("{\"error\""))
            {
                var error = Json.Deserialize<ErrorResult>(response.Content);
                var code = error.Error["code"];
                var message = error.Error["message"];

                var errorMessage = string.Format("XBMC Json Error. Code = {0}, Message: {1}", code, message);
                throw new XbmcJsonException(errorMessage);
            }
        }
    }
}
