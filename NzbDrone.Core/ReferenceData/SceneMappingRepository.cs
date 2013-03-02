using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ReferenceData
{
    public interface ISceneMappingRepository : IBasicRepository<SceneMapping>
    {
        SceneMapping FindByTvdbId(int tvdbId);
        SceneMapping FindByCleanTitle(string cleanTitle);

    }

    public class SceneMappingRepository : BasicRepository<SceneMapping>, ISceneMappingRepository
    {
        public SceneMappingRepository(IObjectDatabase objectDatabase)
            : base(objectDatabase)
        {
        }

        public SceneMapping FindByTvdbId(int tvdbId)
        {
            return Queryable.SingleOrDefault(c => c.TvdbId == tvdbId);
        }

        public SceneMapping FindByCleanTitle(string cleanTitle)
        {
            return Queryable.SingleOrDefault(c => c.CleanTitle == cleanTitle);
        }
    }
}