using System;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Notifications;
using NzbDrone.SignalR;
using Sonarr.Http;

namespace Sonarr.Api.V3.Notifications
{
    [V3ApiController]
    public class NotificationController : ProviderControllerBase<NotificationResource, NotificationBulkResource, INotification, NotificationDefinition>
    {
        public static readonly NotificationResourceMapper ResourceMapper = new ();
        public static readonly NotificationBulkResourceMapper BulkResourceMapper = new ();

        public NotificationController(IBroadcastSignalRMessage signalRBroadcaster, NotificationFactory notificationFactory)
            : base(signalRBroadcaster, notificationFactory, "notification", ResourceMapper, BulkResourceMapper)
        {
        }

        [NonAction]
        public override ActionResult<NotificationResource> UpdateProvider([FromBody] NotificationBulkResource providerResource)
        {
            throw new NotImplementedException();
        }

        [NonAction]
        public override object DeleteProviders([FromBody] NotificationBulkResource resource)
        {
            throw new NotImplementedException();
        }
    }
}
