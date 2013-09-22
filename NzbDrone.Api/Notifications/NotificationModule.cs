using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.REST;
using NzbDrone.Common.Reflection;
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
            GetResourceById = GetNotification;
            CreateResource = Create;
            UpdateResource = Update;
            DeleteResource = DeleteNotification;
        }

        private NotificationResource GetNotification(int id)
        {
            return _notificationService.Get(id).InjectTo<NotificationResource>();
        }

        private List<NotificationResource> GetAll()
        {
            var notifications = _notificationService.All();

            var result = new List<NotificationResource>(notifications.Count);

            foreach (var notification in notifications)
            {
                var notificationResource = new NotificationResource();
                notificationResource.InjectFrom(notification);
                notificationResource.Fields = SchemaBuilder.ToSchema(notification.Settings);
                notificationResource.TestCommand = String.Format("test{0}", notification.Implementation.ToLowerInvariant());

                result.Add(notificationResource);
            }

            return result;
        }

        private int Create(NotificationResource notificationResource)
        {
            var notification = ConvertToNotification(notificationResource);
            return _notificationService.Create(notification).Id;
        }

        private void Update(NotificationResource notificationResource)
        {
            var notification = ConvertToNotification(notificationResource);
            notification.Id = notificationResource.Id;
            _notificationService.Update(notification);
        }

        private void DeleteNotification(int id)
        {
            _notificationService.Delete(id);
        }

        private Notification ConvertToNotification(NotificationResource notificationResource)
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

            //var configType = ReflectionExtensions.CoreAssembly.FindTypeByName(notification)

            //notification.Settings = SchemaBuilder.ReadFormSchema(notification.Settings, notificationResource.Fields);

            return notification;
        }
    }
}