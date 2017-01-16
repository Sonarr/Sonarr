using Newtonsoft.Json;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public class SceneMapping : ModelBase
    {
        public string Title { get; set; }
        public string ParseTerm { get; set; }

        [JsonProperty("searchTitle")]
        public string SearchTerm { get; set; }

        public int TvdbId { get; set; }

        [JsonProperty("season")]
        public int? SeasonNumber { get; set; }

        public int? SceneSeasonNumber { get; set; }
        public string Type { get; set; }
    }
}