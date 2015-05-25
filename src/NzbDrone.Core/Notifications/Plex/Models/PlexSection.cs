using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex.Models
{
    public class PlexSectionDetails
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Language { get; set; }
    }

    public class PlexSection
    {
        [JsonProperty("key")]
        public int Id { get; set; }

        public string Type { get; set; }
        public string Language { get; set; }

        [JsonProperty("_children")]
        public List<PlexSectionDetails> Sections { get; set; }
    }

    public class PlexMediaContainer
    {
        [JsonProperty("_children")]
        public List<PlexSection> Directories { get; set; }
    }
}
