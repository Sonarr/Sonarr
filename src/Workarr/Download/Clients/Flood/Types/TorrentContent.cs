using Newtonsoft.Json;

namespace Workarr.Download.Clients.Flood.Types
{
    public sealed class TorrentContent
    {
        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }
    }
}
