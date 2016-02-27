using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationConfig
    {
        [JsonProperty(PropertyName = "default_destination")]
        public string DefaultDestination { get; set; }
    }
}
