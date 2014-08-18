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
        List<IIndexer> RssEnabled();
        List<IIndexer> SearchEnabled();
    }

    public class IndexerFactory : ProviderFactory<IIndexer, IndexerDefinition>, IIndexerFactory
    {
        public IndexerFactory(IIndexerRepository providerRepository,
                              IEnumerable<IIndexer> providers,
                              IContainer container, 
                              IEventAggregator eventAggregator,
                              Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
        }

        protected override void InitializeProviders()
        {

        }

        protected override List<IndexerDefinition> Active()
        {
            return base.Active().Where(c => c.Enable).ToList();
        }

        public override IndexerDefinition GetProviderCharacteristics(IIndexer provider, IndexerDefinition definition)
        {
            definition = base.GetProviderCharacteristics(provider, definition);

            definition.Protocol = provider.Protocol;
            definition.SupportsRss = provider.SupportsRss;
            definition.SupportsSearch = provider.SupportsSearch;

            return definition;
        }

        public List<IIndexer> RssEnabled()
        {
            return GetAvailableProviders().Where(n => ((IndexerDefinition)n.Definition).EnableRss).ToList();
        }

        public List<IIndexer> SearchEnabled()
        {
            return GetAvailableProviders().Where(n => ((IndexerDefinition)n.Definition).EnableSearch).ToList();
        }        
    }
}