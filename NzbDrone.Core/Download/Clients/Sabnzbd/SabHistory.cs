using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabHistory
    {
        public bool Paused { get; set; }

        [JsonProperty(PropertyName = "slots")]
        public List<SabHistoryItem> Items { get; set; }
    }
}
