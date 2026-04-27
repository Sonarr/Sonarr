using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Notifications;
using NzbDrone.SignalR;
using Sonarr.Api.V5.Provider;
using Sonarr.Http;

namespace Sonarr.Api.V5.Connections;

[V5ApiController]
public class ConnectionController : ProviderControllerBase<ConnectionResource, ConnectionBulkResource, INotification, NotificationDefinition>
{
    public static readonly ConnectionResourceMapper ResourceMapper = new();
    public static readonly ConnectionBulkResourceMapper BulkResourceMapper = new();

    public ConnectionController(IBroadcastSignalRMessage signalRBroadcaster, NotificationFactory notificationFactory)
        : base(signalRBroadcaster, notificationFactory, "connection", ResourceMapper, BulkResourceMapper)
    {
    }

    [NonAction]
    public override Task<Results<Ok<IEnumerable<ConnectionResource>>, BadRequest>> UpdateProvider([FromBody] ConnectionBulkResource providerResource, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    [NonAction]
    public override Task<NoContent> DeleteProviders([FromBody] ConnectionBulkResource resource, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
