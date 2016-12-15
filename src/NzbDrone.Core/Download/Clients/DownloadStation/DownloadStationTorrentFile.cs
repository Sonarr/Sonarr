using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NzbDrone.Core.Download.Clients.DownloadStation.DownloadStationTorrent;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationTorrentFile
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
