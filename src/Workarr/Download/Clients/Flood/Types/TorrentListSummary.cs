using Newtonsoft.Json;

namespace Workarr.Download.Clients.Flood.Types
{
    public sealed class TorrentListSummary
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "torrents")]
        public Dictionary<string, Torrent> Torrents { get; set; }
    }
}
