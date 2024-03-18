using Newtonsoft.Json;
using NzbDrone.Core.Download.Clients.LibTorrent.Models;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    /// <summary> Implements the ListSessionsSettings response from <a href="https://github.com/porla/porla/blob/v0.37.0/src/methods/sessions/sessionssettingslist_reqres.hpp">sessionssettingslist_reqres.hpp</a> </summary>
    public sealed class ResponsePorlaSessionSettingsList
    {
        [JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
        public PorlaSessionSettings Settings { get; set; }
    }

    /// <summary> Wraps the LibTorrentSettingsPack type </summary>
    /// <see cref="LibTorrentSettingsPack"/>
    public sealed class PorlaSessionSettings : LibTorrentSettingsPack
    {
    }
}
