using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationTaskFile
    {
        public string FileName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadStationPriority Priority { get; set; }

        [JsonProperty("size")]
        public long TotalSize { get; set; }

        [JsonProperty("size_downloaded")]
        public long BytesDownloaded { get; set; }
    }
}
