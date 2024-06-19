using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Plex.PlexTv
{
    public class PlexTvResource
    {
        public string Name { get; set; }
        public bool Owned { get; set; }

        public List<PlexTvResourceConnection> Connections { get; set; }

        [JsonProperty("provides")]
        public string ProvidesRaw { get; set; }

        [JsonIgnore]
        public List<string> Provides => ProvidesRaw.Split(",").ToList();
    }

    public class PlexTvResourceConnection
    {
        public string Uri { get; set; }
        public string Protocol { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public bool Local { get; set; }
        public string Host => Uri.IsNullOrWhiteSpace() ? Address : new Uri(Uri).Host;
    }
}
