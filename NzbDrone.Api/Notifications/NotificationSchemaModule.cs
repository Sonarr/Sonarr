using System;
using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
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

            GetResourceAll = GetSchema;
        }

        private List<NotificationResource> GetSchema()
        {
            var notifications = _notificationService.Schema();

            var result = new List<NotificationResource>(notifications.Count);

            foreach (var notification in notifications)
            {
                var notificationResource = new NotificationResource();
                notificationResource.InjectFrom(notification);
                notificationResource.Fields = SchemaBuilder.GenerateSchema(notification.Settings);
                notificationResource.TestCommand = String.Format("test{0}", notification.Implementation.ToLowerInvariant());

                result.Add(notificationResource);
            }

            return result;
        }
    }
}