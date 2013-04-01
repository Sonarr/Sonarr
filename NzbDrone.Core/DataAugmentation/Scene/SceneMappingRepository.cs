using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingRepository : IBasicRepository<SceneMapping>
    {
        SceneMapping FindByTvdbId(int tvdbId);
        SceneMapping FindByCleanTitle(string cleanTitle);

    }

    public class SceneMappingRepository : BasicRepository<SceneMapping>, ISceneMappingRepository
    {
        public SceneMappingRepository(IDatabase database)
            : base(database)
        {
        }

        public SceneMapping FindByTvdbId(int tvdbId)
        {
            return Query.SingleOrDefault(c => c.TvdbId == tvdbId);
        }

        public SceneMapping FindByCleanTitle(string cleanTitle)
        {
            return Query.SingleOrDefault(c => c.CleanTitle == cleanTitle);
        }
    }
}