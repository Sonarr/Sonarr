using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationTaskStatusExtra
    {
        [JsonProperty(PropertyName = "error_detail")]
        public string ErrorDetail { get; set; }

        [JsonProperty(PropertyName = "unzip_progress")]
        public int UnzipProgress { get; set; }
    }
}
