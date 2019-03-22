using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexSectionLocation
    {
        public int Id { get; set; }
        public string Path { get; set; }
    }

    public class PlexSection
    {
        public PlexSection()
        {
            Locations = new List<PlexSectionLocation>();
        }

        [JsonProperty("key")]
        public int Id { get; set; }

        public string Type { get; set; }
        public string Language { get; set; }

        [JsonProperty("Location")]
        public List<PlexSectionLocation> Locations { get; set; }
    }

    public class PlexSectionsContainer
    {
        public PlexSectionsContainer()
        {
            Sections = new List<PlexSection>();
        }

        [JsonProperty("Directory")]
        public List<PlexSection> Sections { get; set; }
    }

    public class PlexSectionLegacy
    {
        [JsonProperty("key")]
        public int Id { get; set; }

        public string Type { get; set; }
        public string Language { get; set; }

        [JsonProperty("_children")]
        public List<PlexSectionLocation> Locations { get; set; }
    }

    public class PlexMediaContainerLegacy
    {
        [JsonProperty("_children")]
        public List<PlexSectionLegacy> Sections { get; set; }
    }
}
