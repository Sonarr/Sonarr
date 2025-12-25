using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportListRepository : IProviderRepository<ImportListDefinition>
    {
        void UpdateSettings(ImportListDefinition model);
        Task UpdateSettingsAsync(ImportListDefinition model, CancellationToken cancellationToken = default);
    }

    public class ImportListRepository : ProviderRepository<ImportListDefinition>, IImportListRepository
    {
        public ImportListRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void UpdateSettings(ImportListDefinition model)
        {
            SetFields(model, m => m.Settings);
        }

        public async Task UpdateSettingsAsync(ImportListDefinition model, CancellationToken cancellationToken = default)
        {
            await SetFieldsAsync(model, cancellationToken, m => m.Settings).ConfigureAwait(false);
        }
    }
}
