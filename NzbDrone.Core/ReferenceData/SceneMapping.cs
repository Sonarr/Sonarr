using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ReferenceData
{
    public class SceneMapping : ModelBase
    {
        public string CleanTitle { get; set; }
        public string SceneName { get; set; }
        public int TvdbId { get; set; }
        public int SeasonNumber { get; set; }
    }
}