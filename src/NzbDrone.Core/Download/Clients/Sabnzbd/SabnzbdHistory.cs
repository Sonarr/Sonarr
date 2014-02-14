using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdHistory
    {
        public bool Paused { get; set; }

        [JsonProperty(PropertyName = "slots")]
        public List<SabnzbdHistoryItem> Items { get; set; }
    }
}
