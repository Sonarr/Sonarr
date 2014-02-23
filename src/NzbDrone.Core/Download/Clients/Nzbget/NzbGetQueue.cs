using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetQueue
    {
        public String Version { get; set; }

        [JsonProperty(PropertyName = "result")]
        public List<NzbgetQueueItem> QueueItems { get; set; }
    }
}
