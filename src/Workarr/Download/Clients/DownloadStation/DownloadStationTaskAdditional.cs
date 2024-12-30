using Newtonsoft.Json;

namespace Workarr.Download.Clients.DownloadStation
{
    public class DownloadStationTaskAdditional
    {
        public Dictionary<string, string> Detail { get; set; }

        public Dictionary<string, string> Transfer { get; set; }

        [JsonProperty("File")]
        public List<DownloadStationTaskFile> Files { get; set; }
    }
}
