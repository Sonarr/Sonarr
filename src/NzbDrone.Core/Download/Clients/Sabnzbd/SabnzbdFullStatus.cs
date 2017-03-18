using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdFullStatus
    {
        // Added in Sabnzbd 2.0.0, my_home was previously in &mode=queue.
        // This is the already resolved completedir path.
        [JsonProperty(PropertyName = "completedir")]
        public string CompleteDir { get; set; }
    }
}
