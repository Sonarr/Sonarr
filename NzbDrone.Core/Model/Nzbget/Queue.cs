using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NzbDrone.Core.Model.Nzbget
{
    public class Queue
    {
        public String Version { get; set; }

        [JsonProperty(PropertyName = "result")]
        public List<QueueItem> QueueItems { get; set; }
    }
}
