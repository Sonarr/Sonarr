using System.Collections.Generic;
using NLog;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerService : IProviderFactory<IIndexer, IndexerDefinition>
    {

    }

    public class IndexerService : ProviderFactory<IIndexer, IndexerDefinition>
    {
        public IndexerService(IProviderRepository<IndexerDefinition> providerRepository, IEnumerable<IIndexer> providers, Logger logger)
            : base(providerRepository, providers, logger)
        {
        }
    }
}