using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexUser
    {
        [JsonProperty("authentication_token")]
        public string AuthenticationToken { get; set; }
    }
}
