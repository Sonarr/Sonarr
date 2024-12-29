using Workarr.Datastore;
using Workarr.Messaging.Events;
using Workarr.Parser.Model;

namespace Workarr.ImportLists.ImportListItems
{
    public interface IImportListItemInfoRepository : IBasicRepository<ImportListItemInfo>
    {
        List<ImportListItemInfo> GetAllForLists(List<int> listIds);
        bool Exists(int tvdbId, string imdbId);
    }

    public class ImportListItemRepository : BasicRepository<ImportListItemInfo>, IImportListItemInfoRepository
    {
        public ImportListItemRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<ImportListItemInfo> GetAllForLists(List<int> listIds)
        {
            return Query(x => listIds.Contains(x.ImportListId));
        }

        public bool Exists(int tvdbId, string imdbId)
        {
            List<ImportListItemInfo> items;

            if (string.IsNullOrWhiteSpace(imdbId))
            {
                items = Query(x => x.TvdbId == tvdbId);
            }
            else
            {
                items = Query(x => x.TvdbId == tvdbId || x.ImdbId == imdbId);
            }

            return items.Any();
        }
    }
}
