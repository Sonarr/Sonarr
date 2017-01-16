using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.UTorrent
{
    public class UTorrentResponse
    {
        public int Build { get; set; }
        public List<UTorrentTorrent> Torrents { get; set; }
        public List<string[]> Label { get; set; }
        public List<object> RssFeeds { get; set; }
        public List<object> RssFilters { get; set; }

        [JsonProperty(PropertyName = "torrentp")]
        public List<UTorrentTorrent> TorrentsChanged { get; set; }
        [JsonProperty(PropertyName = "torrentm")]
        public List<string> TorrentsRemoved { get; set; }
        [JsonProperty(PropertyName = "torrentc")]
        public string CacheNumber { get; set; }

        public List<object[]> Settings { get; set; }
    }
}
