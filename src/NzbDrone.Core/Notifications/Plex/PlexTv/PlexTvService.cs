using System.Linq;
using System.Net.Http;
using System.Text;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Notifications.Plex.PlexTv
{
    public interface IPlexTvService
    {
        PlexTvPinUrlResponse GetPinUrl();
        PlexTvSignInUrlResponse GetSignInUrl(string callbackUrl, int pinId, string pinCode);
        string GetAuthToken(int pinId);

        HttpRequest GetWatchlist(string authToken);
    }

    public class PlexTvService : IPlexTvService
    {
        private readonly IPlexTvProxy _proxy;
        private readonly IConfigService _configService;

        public PlexTvService(IPlexTvProxy proxy, IConfigService configService)
        {
            _proxy = proxy;
            _configService = configService;
        }

        public PlexTvPinUrlResponse GetPinUrl()
        {
            var clientIdentifier = _configService.PlexClientIdentifier;

            var requestBuilder = new HttpRequestBuilder("https://plex.tv/api/v2/pins")
                                 .Accept(HttpAccept.Json)
                                 .AddQueryParam("X-Plex-Client-Identifier", clientIdentifier)
                                 .AddQueryParam("X-Plex-Product", BuildInfo.AppName)
                                 .AddQueryParam("X-Plex-Platform", "Windows")
                                 .AddQueryParam("X-Plex-Platform-Version", "7")
                                 .AddQueryParam("X-Plex-Device-Name", BuildInfo.AppName)
                                 .AddQueryParam("X-Plex-Version", BuildInfo.Version.ToString())
                                 .AddQueryParam("strong", true);

            requestBuilder.Method = HttpMethod.Post;

            var request = requestBuilder.Build();

            return new PlexTvPinUrlResponse
                   {
                       Url = request.Url.ToString(),
                       Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value)
                   };
        }

        public PlexTvSignInUrlResponse GetSignInUrl(string callbackUrl, int pinId, string pinCode)
        {
            var clientIdentifier = _configService.PlexClientIdentifier;

            var requestBuilder = new HttpRequestBuilder("https://app.plex.tv/auth/hashBang")
                                 .AddQueryParam("clientID", clientIdentifier)
                                 .AddQueryParam("forwardUrl", callbackUrl)
                                 .AddQueryParam("code", pinCode)
                                 .AddQueryParam("context[device][product]", BuildInfo.AppName)
                                 .AddQueryParam("context[device][platform]", "Windows")
                                 .AddQueryParam("context[device][platformVersion]", "7")
                                 .AddQueryParam("context[device][version]", BuildInfo.Version.ToString());

            // #! is stripped out of the URL when building, this works around it.
            requestBuilder.Segments.Add("hashBang", "#!");

            var request = requestBuilder.Build();

            return new PlexTvSignInUrlResponse
                   {
                       OauthUrl = request.Url.ToString(),
                       PinId = pinId
                   };
        }

        public string GetAuthToken(int pinId)
        {
            var authToken = _proxy.GetAuthToken(_configService.PlexClientIdentifier, pinId);

            return authToken;
        }

        public HttpRequest GetWatchlist(string authToken)
        {
            var clientIdentifier = _configService.PlexClientIdentifier;

            var requestBuilder = new HttpRequestBuilder("https://metadata.provider.plex.tv/library/sections/watchlist/all")
                                 .Accept(HttpAccept.Json)
                                 .AddQueryParam("clientID", clientIdentifier)
                                 .AddQueryParam("context[device][product]", BuildInfo.AppName)
                                 .AddQueryParam("context[device][platform]", "Windows")
                                 .AddQueryParam("context[device][platformVersion]", "7")
                                 .AddQueryParam("context[device][version]", BuildInfo.Version.ToString())
                                 .AddQueryParam("includeFields", "title,type,year,ratingKey")
                                 .AddQueryParam("includeElements", "Guid")
                                 .AddQueryParam("sort", "watchlistedAt:desc")
                                 .AddQueryParam("type", (int)PlexMediaType.Show);

            if (!string.IsNullOrWhiteSpace(authToken))
            {
                requestBuilder.AddQueryParam("X-Plex-Token", authToken);
            }

            var request = requestBuilder.Build();

            return request;
        }
    }
}
