using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.ImportListItems
{
    public interface IImportListItemRepository : IBasicRepository<ImportListItemInfo>
    {
        Task<List<ImportListItemInfo>> GetAllForListsAsync(List<int> listIds, CancellationToken cancellationToken = default);
    }

    public class ImportListItemRepository : BasicRepository<ImportListItemInfo>, IImportListItemRepository
    {
        public ImportListItemRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public async Task<List<ImportListItemInfo>> GetAllForListsAsync(List<int> listIds, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(x => listIds.Contains(x.ImportListId), cancellationToken).ConfigureAwait(false);
        }
    }
}
