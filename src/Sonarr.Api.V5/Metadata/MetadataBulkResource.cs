using NzbDrone.Core.Extras.Metadata;
using Sonarr.Api.V5.Provider;

namespace Sonarr.Api.V5.Metadata;

public class MetadataBulkResource : ProviderBulkResource<MetadataBulkResource>
{
}

public class MetadataBulkResourceMapper : ProviderBulkResourceMapper<MetadataBulkResource, MetadataDefinition>
{
}
