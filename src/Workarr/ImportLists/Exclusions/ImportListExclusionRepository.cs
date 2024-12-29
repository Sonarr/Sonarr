using Workarr.Datastore;
using Workarr.Messaging.Events;

namespace Workarr.ImportLists.Exclusions
{
    public interface IImportListExclusionRepository : IBasicRepository<ImportListExclusion>
    {
        ImportListExclusion FindByTvdbId(int tvdbId);
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
    }
}
