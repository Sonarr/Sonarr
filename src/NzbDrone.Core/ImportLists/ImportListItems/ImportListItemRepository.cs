using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.ImportListItems
{
    public interface IImportListItemRepository : IBasicRepository<ImportListItemInfo>
    {
        List<ImportListItemInfo> GetAllForLists(List<int> listIds);
    }

    public class ImportListItemRepository : BasicRepository<ImportListItemInfo>, IImportListItemRepository
    {
        public ImportListItemRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<ImportListItemInfo> GetAllForLists(List<int> listIds)
        {
            return Query(x => listIds.Contains(x.ImportListId));
        }
    }
}
