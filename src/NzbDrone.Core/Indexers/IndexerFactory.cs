using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerFactory : IProviderFactory<IIndexer, IndexerDefinition>
    {

    }

    public class IndexerFactory : ProviderFactory<IIndexer, IndexerDefinition>, IIndexerFactory
    {
        private readonly IIndexerRepository _providerRepository;
        private readonly INewznabTestService _newznabTestService;

        public IndexerFactory(IIndexerRepository providerRepository, IEnumerable<IIndexer> providers, IContainer container, IEventAggregator eventAggregator, INewznabTestService newznabTestService, Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _providerRepository = providerRepository;
            _newznabTestService = newznabTestService;
        }

        protected override void InitializeProviders()
        {
            var definitions = _providers.Where(c => c.Protocol == DownloadProtocol.Usenet)
                .SelectMany(indexer => indexer.DefaultDefinitions);

            var currentProviders = All();

            var newProviders = definitions.Where(def => currentProviders.All(c => c.Implementation != def.Implementation)).ToList();

            if (newProviders.Any())
            {
                _providerRepository.InsertMany(newProviders.Cast<IndexerDefinition>().ToList());
            }
        }

        protected override List<IndexerDefinition> Active()
        {
            return base.Active().Where(c => c.Enable).ToList();
        }

        public override IndexerDefinition Create(IndexerDefinition definition)
        {
            if (definition.Implementation == typeof(Newznab.Newznab).Name)
            {
                var indexer = GetInstance(definition);
                _newznabTestService.Test(indexer);
            }

            return base.Create(definition);
        }
    }
}