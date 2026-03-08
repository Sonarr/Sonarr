using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingRepository : IBasicRepository<SceneMapping>
    {
        List<SceneMapping> FindByTvdbid(int tvdbId);
        List<SceneMapping> GetAllByType(string type);
    }

    public class SceneMappingRepository : BasicRepository<SceneMapping>, ISceneMappingRepository
    {
        public SceneMappingRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<SceneMapping> FindByTvdbid(int tvdbId)
        {
            return Query(x => x.TvdbId == tvdbId);
        }

        public List<SceneMapping> GetAllByType(string type)
        {
            return Query(x => x.Type == type);
        }
    }
}
