using NzbDrone.Core.Notifications;
using Sonarr.Http;

namespace Sonarr.Api.V3.Notifications
{
    [V3ApiController]
    public class NotificationController : ProviderControllerBase<NotificationResource, INotification, NotificationDefinition>
    {
        public static readonly NotificationResourceMapper ResourceMapper = new NotificationResourceMapper();

        public NotificationController(NotificationFactory notificationFactory)
            : base(notificationFactory, "notification", ResourceMapper)
        {
        }
    }
}
