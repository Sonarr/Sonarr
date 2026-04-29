using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.SignalR;
using Sonarr.Api.V5.Provider;
using Sonarr.Http;

namespace Sonarr.Api.V5.Metadata;

[V5ApiController]
public class MetadataController : ProviderControllerBase<MetadataResource, MetadataBulkResource, IMetadata, MetadataDefinition>
{
    public static readonly MetadataResourceMapper ResourceMapper = new();
    public static readonly MetadataBulkResourceMapper BulkResourceMapper = new();

    public MetadataController(IBroadcastSignalRMessage signalRBroadcaster, IMetadataFactory metadataFactory)
        : base(signalRBroadcaster, metadataFactory, "metadata", ResourceMapper, BulkResourceMapper)
    {
    }

    [NonAction]
    public override Task<Results<Ok<IEnumerable<MetadataResource>>, BadRequest>> UpdateProvider([FromBody] MetadataBulkResource providerResource, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    [NonAction]
    public override Task<NoContent> DeleteProviders([FromBody] MetadataBulkResource resource, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
