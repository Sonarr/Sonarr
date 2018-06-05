using System.Net;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.Notifications.Plex.PlexTv
{
    public interface IPlexTvProxy
    {
        PlexTvPinResponse GetPinCode(string clientIdentifier);
        string GetAuthToken(string clientIdentifier, int pinId);
    }

    public class PlexTvProxy : IPlexTvProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public PlexTvProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public PlexTvPinResponse GetPinCode(string clientIdentifier)
        {
            var request = BuildRequest(clientIdentifier);
            request.Method = HttpMethod.POST;
            request.ResourceUrl = "/api/v2/pins";
            request.AddQueryParam("strong", true);

            PlexTvPinResponse response;

            if (!Json.TryDeserialize<PlexTvPinResponse>(ProcessRequest(request), out response))
            {
                response = new PlexTvPinResponse();
            }

            return response;
        }

        public string GetAuthToken(string clientIdentifier, int pinId)
        {
            var request = BuildRequest(clientIdentifier);
            request.ResourceUrl = $"/api/v2/pins/{pinId}";

            PlexTvPinResponse response;

            if (!Json.TryDeserialize<PlexTvPinResponse>(ProcessRequest(request), out response))
            {
                response = new PlexTvPinResponse();
            }

            return response.AuthToken;
        }

        private HttpRequestBuilder BuildRequest(string clientIdentifier)
        {
            var requestBuilder = new HttpRequestBuilder("https://plex.tv")
                                 .Accept(HttpAccept.Json)
                                 .AddQueryParam("X-Plex-Client-Identifier", clientIdentifier)
                                 .AddQueryParam("X-Plex-Product", "Sonarr")
                                 .AddQueryParam("X-Plex-Platform", "Windows")
                                 .AddQueryParam("X-Plex-Platform-Version", "7")
                                 .AddQueryParam("X-Plex-Device-Name", "Sonarr")
                                 .AddQueryParam("X-Plex-Version", BuildInfo.Version.ToString());

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
                throw new NzbDroneClientException(ex.Response.StatusCode, "Unable to connect to plex.tv");
            }
            catch (WebException ex)
            {
                throw new NzbDroneClientException(HttpStatusCode.BadRequest, "Unable to connect to plex.tv");
            }

            return response.Content;
        }
    }
}
