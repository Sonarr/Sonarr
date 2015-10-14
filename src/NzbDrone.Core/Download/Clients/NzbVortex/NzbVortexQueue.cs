using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public class NzbVortexQueue
    {
        [JsonProperty(PropertyName = "nzbs")]
        public List<NzbVortexQueueItem> Items { get; set; }
    }
}
