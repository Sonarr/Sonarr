using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public interface IPlexServerProxy
    {
        List<PlexSection> GetTvSections(PlexServerSettings settings);
        string Version(PlexServerSettings settings);
        void Update(int sectionId, string path, PlexServerSettings settings);
    }

    public class PlexServerProxy : IPlexServerProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public PlexServerProxy(IHttpClient httpClient, IConfigService configService, Logger logger)
        {
            _httpClient = httpClient;
            _configService = configService;
            _logger = logger;
        }

        public List<PlexSection> GetTvSections(PlexServerSettings settings)
        {
            var request = BuildRequest("library/sections", HttpMethod.Get, settings);
            var response = ProcessRequest(request);

            CheckForError(response);

            if (response.Contains("_children"))
            {
                return Json.Deserialize<PlexMediaContainerLegacy>(response)
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

            return Json.Deserialize<PlexResponse<PlexSectionsContainer>>(response)
                       .MediaContainer
                       .Sections
                       .Where(d => d.Type == "show")
                       .ToList();
        }

        public void Update(int sectionId, string path, PlexServerSettings settings)
        {
            var resource = $"library/sections/{sectionId}/refresh";
            var request = BuildRequest(resource, HttpMethod.Get, settings);

            request.AddQueryParam("path", path);

            var response = ProcessRequest(request);

            CheckForError(response);
        }

        public string Version(PlexServerSettings settings)
        {
            var request = BuildRequest("identity", HttpMethod.Get, settings);
            var response = ProcessRequest(request);

            CheckForError(response);

            if (response.Contains("_children"))
            {
                return Json.Deserialize<PlexIdentity>(response)
                           .Version;
            }

            return Json.Deserialize<PlexResponse<PlexIdentity>>(response)
                       .MediaContainer
                       .Version;
        }

        private HttpRequestBuilder BuildRequest(string resource, HttpMethod method, PlexServerSettings settings)
        {
            var scheme = settings.UseSsl ? "https" : "http";

            var requestBuilder = new HttpRequestBuilder($"{scheme}://{settings.Host.ToUrlHost()}:{settings.Port}")
                                 .Accept(HttpAccept.Json)
                                 .AddQueryParam("X-Plex-Client-Identifier", _configService.PlexClientIdentifier)
                                 .AddQueryParam("X-Plex-Product", BuildInfo.AppName)
                                 .AddQueryParam("X-Plex-Platform", "Windows")
                                 .AddQueryParam("X-Plex-Platform-Version", "7")
                                 .AddQueryParam("X-Plex-Device-Name", BuildInfo.AppName)
                                 .AddQueryParam("X-Plex-Version", BuildInfo.Version.ToString());

            if (settings.AuthToken.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("X-Plex-Token", settings.AuthToken);
            }

            requestBuilder.ResourceUrl = resource;
            requestBuilder.Method = method;

            return requestBuilder;
        }

        private string ProcessRequest(HttpRequestBuilder requestBuilder)
        {
            var httpRequest = requestBuilder.Build();

            HttpResponse response;

            _logger.Debug("Url: {0}", httpRequest.Url);

            try
            {
                response = _httpClient.Execute(httpRequest);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new PlexAuthenticationException("Unauthorized - AuthToken is invalid");
                }

                throw new PlexException("Unable to connect to Plex Media Server. Status Code: {0}", ex.Response.StatusCode);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new PlexException("Unable to connect to Plex Media Server, certificate validation failed.", ex);
                }

                throw new PlexException($"Unable to connect to Plex Media Server, {ex.Message}", ex);
            }

            return response.Content;
        }

        private void CheckForError(string response)
        {
            _logger.Trace("Checking for error");

            if (response.IsNullOrWhiteSpace())
            {
                _logger.Trace("No response body returned, no error detected");
                return;
            }

            var error = response.Contains("_children") ?
                        Json.Deserialize<PlexError>(response) :
                        Json.Deserialize<PlexResponse<PlexError>>(response).MediaContainer;

            if (error != null && !error.Error.IsNullOrWhiteSpace())
            {
                throw new PlexException(error.Error);
            }

            _logger.Trace("No error detected");
        }
    }
}
