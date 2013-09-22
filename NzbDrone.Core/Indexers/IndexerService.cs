using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerService : IProviderFactory<IIndexer, IndexerDefinition>
    {

    }

    public class IndexerService : ProviderFactory<IIndexer, IndexerDefinition>
    {
        private readonly IIndexerRepository _providerRepository;
        private readonly IEnumerable<IIndexer> _providers;

        public IndexerService(IIndexerRepository providerRepository, IEnumerable<IIndexer> providers, Logger logger)
            : base(providerRepository, providers, logger)
        {
            _providerRepository = providerRepository;
            _providers = providers;
        }

        protected override void InitializeProviders()
        {
            var definitions = _providers.SelectMany(indexer => indexer.DefaultDefinitions);

            var currentProviders = All();

            var newProviders = definitions.Where(def => currentProviders.All(c => c.Implementation != def.Implementation)).ToList();


            if (newProviders.Any())
            {
                _providerRepository.InsertMany(newProviders.Cast<IndexerDefinition>().ToList());
            }
        }
    }
}