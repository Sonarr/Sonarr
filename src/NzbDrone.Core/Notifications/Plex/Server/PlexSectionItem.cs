using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexSectionItem
    {
        [JsonProperty("ratingKey")]
        public int Id { get; set; }

        public string Title { get; set; }
    }

    public class PlexSectionResponse
    {
        [JsonProperty("Metadata")]
        public List<PlexSectionItem> Items { get; set; }
    }

    public class PlexSectionResponseLegacy
    {
        [JsonProperty("_children")]
        public List<PlexSectionItem> Items { get; set; }
    }
}
