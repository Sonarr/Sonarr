using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeTorrent
    {
        public String Hash { get; set; }
        public String Name { get; set; }
        public String State { get; set; }
        public Double Progress { get; set; }
        public Double Eta { get; set; }
        public String Message { get; set; }

        [JsonProperty(PropertyName = "is_finished")]
        public Boolean IsFinished { get; set; }
        
        // Other paths: What is the difference between  'move_completed_path' and 'move_on_completed_path'?
        /*
        [JsonProperty(PropertyName = "move_completed_path")] 
        public String DownloadPathMoveCompleted { get; set; }
        [JsonProperty(PropertyName = "move_on_completed_path")] 
        public String DownloadPathMoveOnCompleted { get; set; }
        */

        [JsonProperty(PropertyName = "save_path")] 
        public String DownloadPath { get; set; }

        [JsonProperty(PropertyName = "total_size")]
        public Int64 Size { get; set; }

        [JsonProperty(PropertyName = "total_done")]
        public Int64 BytesDownloaded { get; set; }

        [JsonProperty(PropertyName = "time_added")]
        public Double DateAdded { get; set; }

        [JsonProperty(PropertyName = "active_time")]
        public Int32 SecondsDownloading { get; set; }

        [JsonProperty(PropertyName = "ratio")]
        public Double Ratio { get; set; }

        [JsonProperty(PropertyName = "is_auto_managed")]
        public Boolean IsAutoManaged { get; set; }

        [JsonProperty(PropertyName = "stop_at_ratio")]
        public Boolean StopAtRatio { get; set; }

        [JsonProperty(PropertyName = "remove_at_ratio")]
        public Boolean RemoveAtRatio { get; set; }

        [JsonProperty(PropertyName = "stop_ratio")]
        public Double StopRatio { get; set; }
    }
}
