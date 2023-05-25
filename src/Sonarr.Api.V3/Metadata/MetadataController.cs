using NzbDrone.Core.Extras.Metadata;
using Sonarr.Http;

namespace Sonarr.Api.V3.Metadata
{
    [V3ApiController]
    public class MetadataController : ProviderControllerBase<MetadataResource, MetadataBulkResource, IMetadata, MetadataDefinition>
    {
        public static readonly MetadataResourceMapper ResourceMapper = new MetadataResourceMapper();
        public static readonly MetadataBulkResourceMapper BulkResourceMapper = new MetadataBulkResourceMapper();

        public MetadataController(IMetadataFactory metadataFactory)
            : base(metadataFactory, "metadata", ResourceMapper, BulkResourceMapper)
        {
        }
    }
}
