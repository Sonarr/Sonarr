using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.REST;
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
            CreateResource = Create;
            UpdateResource = Update;
            DeleteResource = DeleteNotification;
        }

        private List<NotificationResource> GetAll()
        {
            var notifications = _notificationService.All();

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

        private NotificationResource Create(NotificationResource notificationResource)
        {
            var notification = GetNotification(notificationResource);

            notification = _notificationService.Create(notification);
            notificationResource.Id = notification.Id;

            var response = notification.InjectTo<NotificationResource>();
            response.Fields = SchemaBuilder.GenerateSchema(notification.Settings);

            return response;
        }

        private NotificationResource Update(NotificationResource notificationResource)
        {
            var notification = GetNotification(notificationResource);
            notification.Id = notificationResource.Id;
            notification = _notificationService.Update(notification);

            var response = notification.InjectTo<NotificationResource>();
            response.Fields = SchemaBuilder.GenerateSchema(notification.Settings);

            return response;
        }

        private void DeleteNotification(int id)
        {
            _notificationService.Delete(id);
        }

        private Notification GetNotification(NotificationResource notificationResource)
        {
            var notification = _notificationService.Schema()
                               .SingleOrDefault(i =>
                                        i.Implementation.Equals(notificationResource.Implementation,
                                        StringComparison.InvariantCultureIgnoreCase));

            if (notification == null)
            {
                throw new BadRequestException("Invalid Notification Implementation");
            }

            notification.InjectFrom(notificationResource);
            notification.Settings = SchemaDeserializer.DeserializeSchema(notification.Settings, notificationResource.Fields);

            return notification;
        }
    }
}