using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexPreferences
    {
        [JsonProperty("Setting")]
        public List<PlexPreference> Preferences { get; set; }
    }

    public class PlexPreference
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class PlexPreferencesLegacy
    {
        [JsonProperty("_children")]
        public List<PlexPreference> Preferences { get; set; }
    }
}
