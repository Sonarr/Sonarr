using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class JsonRequest
    {
        [JsonProperty(PropertyName = "method")]
        public String Method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object[] Params { get; set; }
    }
}
