using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexSectionItemGuid
    {
        public string Id { get; set; }
    }

    public class PlexSectionItem
    {
        public PlexSectionItem()
        {
            Guids = new List<PlexSectionItemGuid>();
        }

        [JsonProperty("ratingKey")]
        public string Id { get; set; }

        public string Title { get; set; }

        public int Year { get; set; }

        [JsonProperty("Guid")]
        public List<PlexSectionItemGuid> Guids { get; set; }
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
