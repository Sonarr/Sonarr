using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Plex.PlexTv
{
    public class PlexSectionItemGuid
    {
        public string Id { get; set; }
    }

    public class PlexWatchlistRespone
    {
        [JsonProperty("Metadata")]
        public List<PlexWatchlistItem> Items { get; set; }

        public PlexWatchlistRespone()
        {
            Items = new List<PlexWatchlistItem>();
        }
    }

    public class PlexWatchlistItem
    {
        public PlexWatchlistItem()
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
}
