using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.UTorrent
{
    public class UTorrentResponse
    {
        public Int32 Build { get; set; }
        public List<UTorrentTorrent> Torrents { get; set; }
        public List<String[]> Label { get; set; }
        public List<String> RssFeeds { get; set; }
        public List<String> RssFilters { get; set; }

        [JsonProperty(PropertyName = "torrentc")]
        public String CacheNumber { get; set; }

        public List<Object[]> Settings { get; set; }
    }
}
