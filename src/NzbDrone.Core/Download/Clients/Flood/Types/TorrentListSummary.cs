using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Flood.Types
{
    public sealed class TorrentListSummary
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "torrents")]
        public Dictionary<string, Torrent> Torrents { get; set; }
    }
}
