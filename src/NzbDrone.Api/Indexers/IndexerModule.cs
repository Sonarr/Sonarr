using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.Indexers
{
    public class IndexerModule : ProviderModuleBase<IndexerResource, IIndexer, IndexerDefinition>
    {
        public IndexerModule(IndexerFactory indexerFactory)
            : base(indexerFactory, "indexer")
        {
        }

        protected override void Validate(IndexerDefinition definition)
        {
            if (!definition.Enable) return;
            base.Validate(definition);
        }
    }
}