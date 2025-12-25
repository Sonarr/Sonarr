using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingRepository : IBasicRepository<SceneMapping>
    {
        List<SceneMapping> FindByTvdbid(int tvdbId);
        void Clear(string type);

        // Async methods
        Task<List<SceneMapping>> FindByTvdbidAsync(int tvdbId, CancellationToken cancellationToken = default);
        Task ClearAsync(string type, CancellationToken cancellationToken = default);
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

        public void Clear(string type)
        {
            Delete(s => s.Type == type);
        }

        // Async methods
        public async Task<List<SceneMapping>> FindByTvdbidAsync(int tvdbId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(x => x.TvdbId == tvdbId, cancellationToken).ConfigureAwait(false);
        }

        public async Task ClearAsync(string type, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(s => s.Type == type, cancellationToken).ConfigureAwait(false);
        }
    }
}
