using NzbDrone.Core.Indexers;
using Sonarr.Http;

namespace Sonarr.Api.V3.Indexers
{
    [V3ApiController]
    public class IndexerController : ProviderControllerBase<IndexerResource, IndexerBulkResource, IIndexer, IndexerDefinition>
    {
        public static readonly IndexerResourceMapper ResourceMapper = new ();
        public static readonly IndexerBulkResourceMapper BulkResourceMapper = new ();

        public IndexerController(IndexerFactory indexerFactory)
            : base(indexerFactory, "indexer", ResourceMapper, BulkResourceMapper)
        {
        }
    }
}
