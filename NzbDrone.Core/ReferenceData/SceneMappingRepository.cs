using System.Data;
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
        public SceneMappingRepository(IDbConnection database)
            : base(database)
        {
        }

        public SceneMapping FindByTvdbId(int tvdbId)
        {
            return SingleOrDefault(c => c.TvdbId == tvdbId);
        }

        public SceneMapping FindByCleanTitle(string cleanTitle)
        {
            return SingleOrDefault(c => c.CleanTitle == cleanTitle);
        }
    }
}