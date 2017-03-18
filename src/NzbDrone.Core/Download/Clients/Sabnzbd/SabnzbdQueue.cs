using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdQueue
    {
        // Removed in Sabnzbd 2.0.0, see mode=fullstatus instead.
        [JsonProperty(PropertyName = "my_home")]
        public string DefaultRootFolder { get; set; }

        public bool Paused { get; set; }

        [JsonProperty(PropertyName = "slots")]
        public List<SabnzbdQueueItem> Items { get; set; }
    }
}
