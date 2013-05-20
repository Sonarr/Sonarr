using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Core.Notifications;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Notifications
{
    public class NotificationModule : NzbDroneRestModule<NotificationResource>
    {
        private readonly INotificationService _notificationService;

        public NotificationModule(INotificationService notificationService)
        {
            _notificationService = notificationService;
            GetResourceAll = GetAll;
        }

        private List<NotificationResource> GetAll()
        {
            var notifications = _notificationService.All();

            var result = new List<NotificationResource>(notifications.Count);

            foreach (var notification in notifications)
            {
                var indexerResource = new NotificationResource();
                indexerResource.InjectFrom(notification);
                indexerResource.Fields = SchemaBuilder.GenerateSchema(notification.Settings);

                result.Add(indexerResource);
            }

            return result;
        }
    }
}