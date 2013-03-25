using System.Linq;
using NzbDrone.Core.Datastore;
using ServiceStack.DataAnnotations;

namespace NzbDrone.Core.ReferenceData
{
    [Alias("SceneMappings")]
    public class SceneMapping : ModelBase
    {
        public string CleanTitle { get; set; }
        public int TvdbId { get; set; }
        public string SceneName { get; set; }
        public int SeasonNumber { get; set; }
    }
}