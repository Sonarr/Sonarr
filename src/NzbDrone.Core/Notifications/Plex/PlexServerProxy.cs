using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Plex.Models;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Notifications.Plex
{
    public interface IPlexServerProxy
    {
        List<PlexSection> GetTvSections(PlexServerSettings settings);
        void Update(int sectionId, PlexServerSettings settings);
        void UpdateSeries(int metadataId, PlexServerSettings settings);
        string Version(PlexServerSettings settings);
        List<PlexPreference> Preferences(PlexServerSettings settings);
        int? GetMetadataId(int sectionId, int tvdbId, string language, PlexServerSettings settings);
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

        public List<PlexSection> GetTvSections(PlexServerSettings settings)
        {
            var request = GetPlexServerRequest("library/sections", Method.GET, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Sections response: {0}", response.Content);
            CheckForError(response);

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

            _logger.Trace("Update response: {0}", response.Content);
            CheckForError(response);
        }

        public void UpdateSeries(int metadataId, PlexServerSettings settings)
        {
            var resource = String.Format("library/metadata/{0}/refresh", metadataId);
            var request = GetPlexServerRequest(resource, Method.PUT, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Update Series response: {0}", response.Content);
            CheckForError(response);
        }

        public string Version(PlexServerSettings settings)
        {
            var request = GetPlexServerRequest("identity", Method.GET, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Version response: {0}", response.Content);
            CheckForError(response);

            return Json.Deserialize<PlexIdentity>(response.Content).Version;
        }

        public List<PlexPreference> Preferences(PlexServerSettings settings)
        {
            var request = GetPlexServerRequest(":/prefs", Method.GET, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Preferences response: {0}", response.Content);
            CheckForError(response);

            return Json.Deserialize<PlexPreferences>(response.Content).Preferences;
        }

        public int? GetMetadataId(int sectionId, int tvdbId, string language, PlexServerSettings settings)
        {
            var guid = String.Format("com.plexapp.agents.thetvdb://{0}?lang={1}", tvdbId, language);
            var resource = String.Format("library/sections/{0}/all?guid={1}", sectionId, System.Web.HttpUtility.UrlEncode(guid));
            var request = GetPlexServerRequest(resource, Method.GET, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Sections response: {0}", response.Content);
            CheckForError(response);

            var item = Json.Deserialize<PlexSectionResponse>(response.Content)
                           .Items
                           .FirstOrDefault();

            if (item == null)
            {
                return null;
            }

            return item.Id;
        }

        private String Authenticate(string username, string password)
        {
            var request = GetMyPlexRequest("users/sign_in.json", Method.POST);
            var client = GetMyPlexClient(username, password); 

            var response = client.Execute(request);

            _logger.Debug("Authentication Response: {0}", response.Content);
            CheckForError(response);

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
	        request.AddHeader("X-Plex-Product", "Sonarr");
            request.AddHeader("X-Plex-Version", BuildInfo.Version.ToString());

            return request;
        }

        private RestClient GetPlexServerClient(PlexServerSettings settings)
        {
            var protocol = settings.UseSsl ? "https" : "http";

            return RestClientFactory.BuildClient(String.Format("{0}://{1}:{2}", protocol, settings.Host, settings.Port));
        }

        private RestRequest GetPlexServerRequest(string resource, Method method, PlexServerSettings settings)
        {
            var request = new RestRequest(resource, method);
            request.AddHeader("Accept", "application/json");

            if (!settings.Username.IsNullOrWhiteSpace())
            {
                request.AddParameter("X-Plex-Token", GetAuthenticationToken(settings.Username, settings.Password), ParameterType.HttpHeader);
            }

            return request;
        }

        private string GetAuthenticationToken(string username, string password)
        {
            return _authCache.Get(username, () => Authenticate(username, password));
        }

        private void CheckForError(IRestResponse response)
        {
            _logger.Trace("Checking for error");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new PlexException("Unauthorized");
            }

            var error = Json.Deserialize<PlexError>(response.Content);

            if (error != null && !error.Error.IsNullOrWhiteSpace())
            {
                throw new PlexException(error.Error);
            }

            _logger.Trace("No error detected");
        }
    }
}
