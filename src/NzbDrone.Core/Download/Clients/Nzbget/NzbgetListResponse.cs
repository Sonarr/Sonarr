using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetListResponse<T>
    {
        public String Version { get; set; }

        [JsonProperty(PropertyName = "result")]
        public List<T> QueueItems { get; set; }
    }
}
