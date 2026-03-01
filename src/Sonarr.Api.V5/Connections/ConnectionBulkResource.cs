using NzbDrone.Core.Notifications;
using Sonarr.Api.V5.Provider;

namespace Sonarr.Api.V5.Connections;

public class ConnectionBulkResource : ProviderBulkResource<ConnectionBulkResource>
{
}

public class ConnectionBulkResourceMapper : ProviderBulkResourceMapper<ConnectionBulkResource, NotificationDefinition>
{
}
