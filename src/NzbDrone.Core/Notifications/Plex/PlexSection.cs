using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexSection
    {
        public Int32 Id { get; set; }
        public String Path { get; set; }
    }

    public class PlexDirectory
    {
        public String Type { get; set; }

        [JsonProperty("_children")]
        public List<PlexSection> Sections { get; set; }
    }

    public class PlexMediaContainer
    {
        [JsonProperty("_children")]
        public List<PlexDirectory> Directories { get; set; }
    }
}
