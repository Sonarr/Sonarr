using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabAddResponse
    {
        public bool Status { get; set; }

        [JsonProperty(PropertyName = "nzo_ids")]
        public List<String> Ids { get; set; }
    }
}
