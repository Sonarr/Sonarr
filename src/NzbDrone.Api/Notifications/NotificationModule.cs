using NzbDrone.Core.Notifications;

namespace NzbDrone.Api.Notifications
{
    public class NotificationModule : ProviderModuleBase<NotificationResource, INotification, NotificationDefinition>
    {
        public NotificationModule(NotificationFactory notificationFactory)
            : base(notificationFactory, "notification")
        {
        }

        protected override void Validate(NotificationDefinition definition)
        {
            if (!definition.OnGrab && !definition.OnDownload) return;
            base.Validate(definition);
        }
    }
}