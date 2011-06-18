using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("SceneMappings")]
    [PrimaryKey("CleanTitle", autoIncrement = false)]
    public class SceneMapping
    {
        public virtual string CleanTitle { get; set; }

        public virtual int SeriesId { get; set; }

        public virtual string SceneName { get; set; }
    }
}
