using System;
using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Core.Notifications;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Notifications
{
    public class NotificationSchemaModule : NzbDroneRestModule<NotificationResource>
    {
        private readonly INotificationFactory _notificationFactory;

        public NotificationSchemaModule(INotificationFactory notificationFactory)
            : base("notification/schema")
        {
            _notificationFactory = notificationFactory;
            GetResourceAll = GetSchema;
        }

        private List<NotificationResource> GetSchema()
        {
            var notifications = _notificationFactory.Templates();

            var result = new List<NotificationResource>(notifications.Count);

            foreach (var notification in notifications)
            {
                var notificationResource = new NotificationResource();
                notificationResource.InjectFrom(notification);
                notificationResource.Fields = SchemaBuilder.ToSchema(notification.Settings);

                result.Add(notificationResource);
            }

            return result;
        }
    }
}