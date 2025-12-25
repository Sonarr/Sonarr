using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.Exclusions
{
    public interface IImportListExclusionRepository : IBasicRepository<ImportListExclusion>
    {
        ImportListExclusion FindByTvdbId(int tvdbId);

        // Async
        Task<ImportListExclusion> FindByTvdbIdAsync(int tvdbId, CancellationToken cancellationToken = default);
    }

    public class ImportListExclusionRepository : BasicRepository<ImportListExclusion>, IImportListExclusionRepository
    {
        public ImportListExclusionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public ImportListExclusion FindByTvdbId(int tvdbId)
        {
            return Query(m => m.TvdbId == tvdbId).SingleOrDefault();
        }

        public async Task<ImportListExclusion> FindByTvdbIdAsync(int tvdbId, CancellationToken cancellationToken = default)
        {
            var importListExclusions = await QueryAsync(m => m.TvdbId == tvdbId, cancellationToken).ConfigureAwait(false);
            return importListExclusions.SingleOrDefault();
        }
    }
}
