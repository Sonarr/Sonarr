using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetTorrentQuery
    {
        public BroadcastheNetTorrentQuery()
        {
            Origin = new List<string>();
        }
        
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Category { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Search { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Codec { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Container { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Source { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Resolution { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Origin { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Hash { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Tvdb { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Tvrage { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Age { get; set; }

        public BroadcastheNetTorrentQuery Clone()
        {
            return MemberwiseClone() as BroadcastheNetTorrentQuery;
        }
    }
}
