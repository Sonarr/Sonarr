using Newtonsoft.Json;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("SceneMappings")]
    [PrimaryKey("CleanTitle", autoIncrement = false)]
    public class SceneMapping
    {
        public string CleanTitle { get; set; }

        [JsonProperty(PropertyName = "Id")]
        public int SeriesId { get; set; }

        [JsonProperty(PropertyName = "Title")]
        public string SceneName { get; set; }
    }
}