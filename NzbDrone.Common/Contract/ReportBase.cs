using System;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Common.Contract
{
    public abstract class ReportBase
    {
        [JsonProperty("v")]
        public string Version { get; set; }

        [JsonProperty("p")]
        public bool IsProduction { get; set; }

        [JsonProperty("u")]
        public Guid UGuid { get; set; }
    }
}
