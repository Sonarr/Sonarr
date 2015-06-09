using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Indexers.Rarbg
{
    public class RarbgTorrent
    {
        [JsonProperty("f")]
        public string Title { get; set; }
        [JsonProperty("c")]
        public string Category { get; set; }
        [JsonProperty("d")]
        public string DownloadUrl { get; set; }
        [JsonProperty("s")]
        public int Seeders { get; set; }
        [JsonProperty("l")]
        public int Leechers { get; set; }
        [JsonProperty("t")]
        public long Size { get; set; }
        [JsonProperty("u")]
        public DateTime PublishDate { get; set; }
    }
}
