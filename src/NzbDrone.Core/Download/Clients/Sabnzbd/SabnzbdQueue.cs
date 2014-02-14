using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdQueue
    {
        public bool Paused { get; set; }

        [JsonProperty(PropertyName = "slots")]
        public List<SabnzbdQueueItem> Items { get; set; }
    }
}
