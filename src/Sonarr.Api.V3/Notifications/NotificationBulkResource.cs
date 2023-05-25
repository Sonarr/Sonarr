using NzbDrone.Core.Notifications;

namespace Sonarr.Api.V3.Notifications
{
    public class NotificationBulkResource : ProviderBulkResource<NotificationBulkResource>
    {
    }

    public class NotificationBulkResourceMapper : ProviderBulkResourceMapper<NotificationBulkResource, NotificationDefinition>
    {
    }
}
