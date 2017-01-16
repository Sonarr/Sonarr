using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdHistoryItem
    {
        [JsonProperty(PropertyName = "fail_message")]
        public string FailMessage { get; set; }

        [JsonProperty(PropertyName = "bytes")]
        public long Size { get; set; }
        public string Category { get; set; }

        [JsonProperty(PropertyName = "nzb_name")]
        public string NzbName { get; set; }

        [JsonProperty(PropertyName = "download_time")]
        public int DownloadTime { get; set; }

        public string Storage { get; set; }
        public SabnzbdDownloadStatus Status { get; set; }

        [JsonProperty(PropertyName = "nzo_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Title { get; set; }
    }
}
