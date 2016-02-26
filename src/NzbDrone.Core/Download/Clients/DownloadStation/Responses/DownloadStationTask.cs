using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationTask
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public long Size { get; set; }

        [JsonProperty(PropertyName = "status_extra")]
        public DownloadStationTaskStatusExtra StatusExtra { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadStationTaskStatus Status { get; set; }

        public DownloadStationTaskAdditional Additional { get; set; }
    }
}
