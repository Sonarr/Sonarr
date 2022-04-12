using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexSectionItem
    {
        [JsonProperty("ratingKey")]
        public string Id { get; set; }

        public string Title { get; set; }
        public int Year { get; set; }
        public string Guid { get; set; }
    }

    public class PlexSectionResponse
    {
        [JsonProperty("Metadata")]
        public List<PlexSectionItem> Items { get; set; }

        public PlexSectionResponse()
        {
            Items = new List<PlexSectionItem>();
        }
    }

    public class PlexSectionResponseLegacy
    {
        [JsonProperty("_children")]
        public List<PlexSectionItem> Items { get; set; }

        public PlexSectionResponseLegacy()
        {
            Items = new List<PlexSectionItem>();
        }
    }
}
