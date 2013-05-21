using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Annotations;
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
            UpdateResource = Update;
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

        private NotificationResource Update(NotificationResource notificationResource)
        {
            //Todo: Convert Resource back to Settings

            var notification = _notificationService.Get(notificationResource.Id);

            notification.OnGrab = notificationResource.OnGrab;
            notification.OnDownload = notificationResource.OnDownload;

            var properties = notification.Settings.GetType().GetSimpleProperties();

            foreach (var propertyInfo in properties)
            {
                var fieldAttribute = propertyInfo.GetAttribute<FieldDefinitionAttribute>(false);

                if (fieldAttribute != null)
                {
                    //Find coresponding field

                    var field = notificationResource.Fields.Find(f => f.Name == propertyInfo.Name);

                    propertyInfo.SetValue(notification.Settings, field.Value, null);
                }
            }

            return notificationResource;
        }
    }
}