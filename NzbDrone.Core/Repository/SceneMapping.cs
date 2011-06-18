using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("SceneMappings")]
    [PrimaryKey("CleanTitle", autoIncrement = false)]
    public class SceneMapping
    {
        public string CleanTitle { get; set; }

        public int SeriesId { get; set; }

        public string SceneName { get; set; }
    }
}