using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationInfo
    {
        [JsonProperty(PropertyName = "version_string")]
        public string Version { get; set; }
    }
}
