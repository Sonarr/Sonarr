using Newtonsoft.Json;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public class SceneMapping : ModelBase
    {
        public string CleanTitle { get; set; }

        [JsonProperty("Title")]
        public string SceneName { get; set; }

        [JsonProperty("Id")]
        public int TvdbId { get; set; }
        public int SeasonNumber { get; set; }
    }
}