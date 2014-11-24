using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Notifications.Plex
{
    public interface IPlexServerProxy
    {
        List<PlexDirectory> GetTvSections(PlexServerSettings settings);
        void Update(int sectionId, PlexServerSettings settings);
    }

    public class PlexServerProxy : IPlexServerProxy
    {
        private readonly ICached<String> _authCache;
        private readonly Logger _logger;

        public PlexServerProxy(ICacheManager cacheManager, Logger logger)
        {
            _authCache = cacheManager.GetCache<String>(GetType(), "authCache");
            _logger = logger;
        }

        public List<PlexDirectory> GetTvSections(PlexServerSettings settings)
        {
            var request = GetPlexServerRequest("library/sections", Method.GET, settings);
            var client = GetPlexServerClient(settings);

            var response = client.Execute(request);

            CheckForError(response.Content);
            _logger.Debug("Sections response: {0}", response.Content);

            return Json.Deserialize<PlexMediaContainer>(response.Content)
                       .Directories
                       .Where(d => d.Type == "show")
                       .ToList();
        }

        public void Update(int sectionId, PlexServerSettings settings)
        {
            var resource = String.Format("library/sections/{0}/refresh", sectionId);
            var request = GetPlexServerRequest(resource, Method.GET, settings);
            var client = GetPlexServerClient(settings);

            var response = client.Execute(request);

            CheckForError(response.Content);
            _logger.Debug("Update response: {0}", response.Content);
        }

        private String Authenticate(string username, string password)
        {
            var request = GetMyPlexRequest("users/sign_in.json", Method.POST);
            var client = GetMyPlexClient(username, password); 

            var response = client.Execute(request);

            CheckForError(response.Content);
            _logger.Debug("Authentication Response: {0}", response.Content);

            var user = Json.Deserialize<PlexUser>(JObject.Parse(response.Content).SelectToken("user").ToString());

            _authCache.Set(username, user.AuthenticationToken);

            return user.AuthenticationToken;
        }

        private RestClient GetMyPlexClient(string username, string password)
        {
            var client = RestClientFactory.BuildClient("https://my.plexapp.com");
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            return client;
        }

        private RestRequest GetMyPlexRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method);
	        request.AddHeader("X-Plex-Platform", "Windows");
	        request.AddHeader("X-Plex-Platform-Version", "7");
	        request.AddHeader("X-Plex-Provides", "player");
	        request.AddHeader("X-Plex-Client-Identifier", "AB6CCCC7-5CF5-4523-826A-B969E0FFD8A0");
	        request.AddHeader("X-Plex-Device-Name", "Sonarr");
	        request.AddHeader("X-Plex-Product", "PlexWMC");
	        request.AddHeader("X-Plex-Version", "0");

            return request;
        }

        private RestClient GetPlexServerClient(PlexServerSettings settings)
        {
            return RestClientFactory.BuildClient(String.Format("http://{0}:{1}", settings.Host, settings.Port));
        }

        private RestRequest GetPlexServerRequest(string resource, Method method, PlexServerSettings settings)
        {
            var request = new RestRequest(resource, method);
            request.AddHeader("Accept", "application/json");

            if (!settings.Username.IsNullOrWhiteSpace())
            {
                request.AddParameter("X-Plex-Token", GetAuthenticationToken(settings.Username, settings.Password));
            }

            return request;
        }

        private string GetAuthenticationToken(string username, string password)
        {
            return _authCache.Get(username, () => Authenticate(username, password));
        }

        private void CheckForError(string response)
        {
            var error = Json.Deserialize<PlexError>(response);

            if (error != null && !error.Error.IsNullOrWhiteSpace())
            {
                throw new PlexException(error.Error);
            }
        }
    }
}
