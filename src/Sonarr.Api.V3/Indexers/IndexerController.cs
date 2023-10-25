using NzbDrone.Core.Indexers;
using NzbDrone.Core.Validation;
using Sonarr.Http;

namespace Sonarr.Api.V3.Indexers
{
    [V3ApiController]
    public class IndexerController : ProviderControllerBase<IndexerResource, IndexerBulkResource, IIndexer, IndexerDefinition>
    {
        public static readonly IndexerResourceMapper ResourceMapper = new ();
        public static readonly IndexerBulkResourceMapper BulkResourceMapper = new ();

        public IndexerController(IndexerFactory indexerFactory, DownloadClientExistsValidator downloadClientExistsValidator)
            : base(indexerFactory, "indexer", ResourceMapper, BulkResourceMapper)
        {
            SharedValidator.RuleFor(c => c.DownloadClientId).SetValidator(downloadClientExistsValidator);
        }
    }
}
