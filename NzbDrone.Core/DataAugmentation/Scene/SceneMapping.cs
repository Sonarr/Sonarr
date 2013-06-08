using Newtonsoft.Json;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public class SceneMapping : ModelBase
    {
        [JsonProperty("CleanTitle")]
        public string ParseTerm { get; set; }

        [JsonProperty("Title")]
        public string SearchTerm { get; set; }

        [JsonProperty("Id")]
        public int TvdbId { get; set; }

        public int SeasonNumber { get; set; }
    }
}