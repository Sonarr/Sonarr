using System.Collections.Generic;
using System.Text.Json.Serialization;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public class SeasonType : IEmbeddedDocument
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<int> SeasonNumbers { get; set; }
        public int EpisodeCount { get; set; }

        [JsonIgnore]
        public static string Official => "official";
    }
}
