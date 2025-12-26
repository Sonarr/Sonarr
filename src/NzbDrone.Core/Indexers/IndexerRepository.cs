using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerRepository : IProviderRepository<IndexerDefinition>
    {
        Task<IndexerDefinition> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    }

    public class IndexerRepository : ProviderRepository<IndexerDefinition>, IIndexerRepository
    {
        public IndexerRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public async Task<IndexerDefinition> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var indexers = await QueryAsync(i => i.Name == name, cancellationToken).ConfigureAwait(false);
            return indexers.SingleOrDefault();
        }
    }
}
