using System.Text;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Notifications.Plex.PlexTv
{
    public interface IPlexTvService
    {
        PlexTvSignInUrlResponse GetSignInUrl(string callbackUrl);
        string GetAuthToken(int pinId);
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

        public PlexTvSignInUrlResponse GetSignInUrl(string callbackUrl)
        {
            var clientIdentifier = _configService.PlexClientIdentifier;
            var pin = _proxy.GetPinCode(clientIdentifier);

            var url = new StringBuilder();
            url.Append("https://app.plex.tv/auth/#!");
            url.Append($"?clientID={clientIdentifier}");
            url.Append($"&forwardUrl={callbackUrl}");
            url.Append($"&code={pin.Code}");
            url.Append($"&context[device][version]=${BuildInfo.Version.ToString()}");
            url.Append("&context[device][product]=Sonarr");
            url.Append("&context[device][platform]=Windows");
            url.Append("&context[device][platformVersion]=7");

            return new PlexTvSignInUrlResponse
                   {
                       OauthUrl = url.ToString(),
                       PinId = pin.Id
                   };
        }

        public string GetAuthToken(int pinId)
        {
            var authToken = _proxy.GetAuthToken(_configService.PlexClientIdentifier, pinId);

            return authToken;
        }
    }
}
