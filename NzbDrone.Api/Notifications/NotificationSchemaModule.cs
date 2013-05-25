using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Notifications;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Notifications
{
    public class NotificationSchemaModule : NzbDroneRestModule<NotificationResource>
        {
        private readonly INotificationService _notificationService;

        public NotificationSchemaModule(INotificationService notificationService)
            : base("notification/schema")
        {
            _notificationService = notificationService;

            GetResourceAll = GetAll;
        }

        private List<NotificationResource> GetAll()
        {
            //Need to get all the possible Notification's same as we would for settiings (but keep them empty)

            var notifications = _notificationService.Schema();

            var result = new List<NotificationResource>(notifications.Count);

            foreach (var notification in notifications)
            {
                var notificationResource = new NotificationResource();
                notificationResource.InjectFrom(notification);
                notificationResource.Fields = SchemaBuilder.GenerateSchema(notification.Settings);

                result.Add(notificationResource);
            }

            return result;
        }
    }
}