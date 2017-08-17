using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationTaskAdditional
    {
        public Dictionary<string, string> Detail { get; set; }

        public Dictionary<string, string> Transfer { get; set; }

        [JsonProperty("File")]
        public List<DownloadStationTaskFile> Files { get; set; }
    }
}
