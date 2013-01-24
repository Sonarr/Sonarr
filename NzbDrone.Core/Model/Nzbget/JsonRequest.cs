using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NzbDrone.Core.Model.Nzbget
{
    public class JsonRequest
    {
        [JsonProperty(PropertyName = "method")]
        public String Method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object[] Params { get; set; }
    }
}
