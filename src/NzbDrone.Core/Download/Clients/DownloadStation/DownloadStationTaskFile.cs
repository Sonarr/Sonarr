using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static NzbDrone.Core.Download.Clients.DownloadStation.DownloadStationTask;

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
