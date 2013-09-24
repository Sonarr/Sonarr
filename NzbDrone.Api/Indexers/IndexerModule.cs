using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.Indexers
{
    public class IndexerModule : ProviderModuleBase<ProviderResource, IIndexer, IndexerDefinition>
    {
        public IndexerModule(IndexerFactory indexerFactory)
            : base(indexerFactory)
        {
        }
    }
}