using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexUser
    {
        [JsonProperty("authentication_token")]
        public String AuthenticationToken { get; set; }
    }
}
