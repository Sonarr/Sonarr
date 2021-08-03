using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeTorrent
    {
        public string Hash { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public double Progress { get; set; }
        public double Eta { get; set; }
        public string Message { get; set; }

        [JsonProperty(PropertyName = "is_finished")]
        public bool IsFinished { get; set; }

        // Other paths: What is the difference between  'move_completed_path' and 'move_on_completed_path'?
        /*
        [JsonProperty(PropertyName = "move_completed_path")]
        public String DownloadPathMoveCompleted { get; set; }
        [JsonProperty(PropertyName = "move_on_completed_path")]
        public String DownloadPathMoveOnCompleted { get; set; }
        */

        [JsonProperty(PropertyName = "save_path")]
        public string DownloadPath { get; set; }

        [JsonProperty(PropertyName = "total_size")]
        public long Size { get; set; }

        [JsonProperty(PropertyName = "total_done")]
        public long BytesDownloaded { get; set; }

        [JsonProperty(PropertyName = "time_added")]
        public double DateAdded { get; set; }

        [JsonProperty(PropertyName = "active_time")]
        public int SecondsDownloading { get; set; }

        [JsonProperty(PropertyName = "ratio")]
        public double Ratio { get; set; }

        [JsonProperty(PropertyName = "is_auto_managed")]
        public bool IsAutoManaged { get; set; }

        [JsonProperty(PropertyName = "stop_at_ratio")]
        public bool StopAtRatio { get; set; }

        [JsonProperty(PropertyName = "remove_at_ratio")]
        public bool RemoveAtRatio { get; set; }

        [JsonProperty(PropertyName = "stop_ratio")]
        public double StopRatio { get; set; }
    }
}
