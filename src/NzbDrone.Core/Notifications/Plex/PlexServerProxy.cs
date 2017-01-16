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
using RestSharp.Authenticators;

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
        private readonly ICached<string> _authCache;
        private readonly Logger _logger;

        public PlexServerProxy(ICacheManager cacheManager, Logger logger)
        {
            _authCache = cacheManager.GetCache<string>(GetType(), "authCache");
            _logger = logger;
        }

        public List<PlexSection> GetTvSections(PlexServerSettings settings)
        {
            var request = GetPlexServerRequest("library/sections", Method.GET, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Sections response: {0}", response.Content);
            CheckForError(response, settings);

            if (response.Content.Contains("_children"))
            {
                return Json.Deserialize<PlexMediaContainerLegacy>(response.Content)
                    .Sections
                    .Where(d => d.Type == "show")
                    .Select(s => new PlexSection
                                 {
                                     Id = s.Id,
                                     Language = s.Language,
                                     Locations = s.Locations,
                                     Type = s.Type
                                 })
                    .ToList();
            }

            return Json.Deserialize<PlexResponse<PlexSectionsContainer>>(response.Content)
                       .MediaContainer
                       .Sections
                       .Where(d => d.Type == "show")
                       .ToList();
        }

        public void Update(int sectionId, PlexServerSettings settings)
        {
            var resource = string.Format("library/sections/{0}/refresh", sectionId);
            var request = GetPlexServerRequest(resource, Method.GET, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Update response: {0}", response.Content);
            CheckForError(response, settings);
        }

        public void UpdateSeries(int metadataId, PlexServerSettings settings)
        {
            var resource = string.Format("library/metadata/{0}/refresh", metadataId);
            var request = GetPlexServerRequest(resource, Method.PUT, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Update Series response: {0}", response.Content);
            CheckForError(response, settings);
        }

        public string Version(PlexServerSettings settings)
        {
            var request = GetPlexServerRequest("identity", Method.GET, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Version response: {0}", response.Content);
            CheckForError(response, settings);

            if (response.Content.Contains("_children"))
            {
                return Json.Deserialize<PlexIdentity>(response.Content)
                           .Version;
            }

            return Json.Deserialize<PlexResponse<PlexIdentity>>(response.Content)
                       .MediaContainer
                       .Version;
        }

        public List<PlexPreference> Preferences(PlexServerSettings settings)
        {
            var request = GetPlexServerRequest(":/prefs", Method.GET, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Preferences response: {0}", response.Content);
            CheckForError(response, settings);

            if (response.Content.Contains("_children"))
            {
                return Json.Deserialize<PlexPreferencesLegacy>(response.Content)
                           .Preferences;
            }

            return Json.Deserialize<PlexResponse<PlexPreferences>>(response.Content)
                       .MediaContainer
                       .Preferences;
        }

        public int? GetMetadataId(int sectionId, int tvdbId, string language, PlexServerSettings settings)
        {
            var guid = string.Format("com.plexapp.agents.thetvdb://{0}?lang={1}", tvdbId, language);
            var resource = string.Format("library/sections/{0}/all?guid={1}", sectionId, System.Web.HttpUtility.UrlEncode(guid));
            var request = GetPlexServerRequest(resource, Method.GET, settings);
            var client = GetPlexServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Sections response: {0}", response.Content);
            CheckForError(response, settings);

            List<PlexSectionItem> items;

            if (response.Content.Contains("_children"))
            {
                items = Json.Deserialize<PlexSectionResponseLegacy>(response.Content)
                            .Items;
            }

            else
            {
                items = Json.Deserialize<PlexResponse<PlexSectionResponse>>(response.Content)
                            .MediaContainer
                            .Items;
            }

            if (items == null || items.Empty())
            {
                return null;
            }

            return items.First().Id;
        }

        private string Authenticate(PlexServerSettings settings)
        {
            var request = GetPlexTvRequest("users/sign_in.json", Method.POST);
            var client = GetPlexTvClient(settings.Username, settings.Password); 

            var response = client.Execute(request);

            _logger.Debug("Authentication Response: {0}", response.Content);
            CheckForError(response, settings);

            var user = Json.Deserialize<PlexUser>(JObject.Parse(response.Content).SelectToken("user").ToString());

            return user.AuthenticationToken;
        }

        private RestClient GetPlexTvClient(string username, string password)
        {
            var client = RestClientFactory.BuildClient("https://plex.tv");
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            return client;
        }

        private RestRequest GetPlexTvRequest(string resource, Method method)
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

            return RestClientFactory.BuildClient(string.Format("{0}://{1}:{2}", protocol, settings.Host, settings.Port));
        }

        private RestRequest GetPlexServerRequest(string resource, Method method, PlexServerSettings settings)
        {
            var request = new RestRequest(resource, method);
            request.AddHeader("Accept", "application/json");

            if (settings.Username.IsNotNullOrWhiteSpace())
            {
                request.AddParameter("X-Plex-Token", GetAuthenticationToken(settings), ParameterType.HttpHeader);
            }

            return request;
        }

        private string GetAuthenticationToken(PlexServerSettings settings)
        {
            var token = _authCache.Get(settings.Username + settings.Password, () => Authenticate(settings));

            if (token.IsNullOrWhiteSpace())
            {
                throw new PlexAuthenticationException("Invalid Token - Update your username and password");
            }

            return token;
        }

        private void CheckForError(IRestResponse response, PlexServerSettings settings)
        {
            _logger.Trace("Checking for error");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (settings.Username.IsNullOrWhiteSpace())
                {
                    throw new PlexAuthenticationException("Unauthorized - Username and password required");
                }

                //Set the token to null in the cache so we don't keep trying with bad credentials
                _authCache.Set(settings.Username + settings.Password, null);
                throw new PlexAuthenticationException("Unauthorized - Username or password is incorrect");
            }

            if (response.Content.IsNullOrWhiteSpace())
            {
                _logger.Trace("No response body returned, no error detected");
                return;
            }

            var error = response.Content.Contains("_children") ?
                        Json.Deserialize<PlexError>(response.Content) : 
                        Json.Deserialize<PlexResponse<PlexError>>(response.Content).MediaContainer;

            if (error != null && !error.Error.IsNullOrWhiteSpace())
            {
                throw new PlexException(error.Error);
            }

            _logger.Trace("No error detected");
        }
    }
}
