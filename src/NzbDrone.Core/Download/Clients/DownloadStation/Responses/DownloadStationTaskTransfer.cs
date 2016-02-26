using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationTaskTransfer
    {
        [JsonProperty(PropertyName = "size_downloaded")]
        public long Downloaded { get; set; }
    }
}
