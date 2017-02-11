using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DSMInfoResponse
    {
        [JsonProperty("serial")]
        public string SerialNumber { get; set; }
    }
}
