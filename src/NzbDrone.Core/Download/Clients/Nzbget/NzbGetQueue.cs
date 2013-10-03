using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbGetQueue
    {
        public String Version { get; set; }

        [JsonProperty(PropertyName = "result")]
        public List<NzbGetQueueItem> QueueItems { get; set; }
    }
}
