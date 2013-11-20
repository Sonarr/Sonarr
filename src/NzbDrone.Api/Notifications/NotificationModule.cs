using NzbDrone.Core.Notifications;

namespace NzbDrone.Api.Notifications
{
    public class IndexerModule : ProviderModuleBase<NotificationResource, INotification, NotificationDefinition>
    {
        public IndexerModule(NotificationFactory notificationrFactory)
            : base(notificationrFactory, "notification")
        {
        }

        protected override void Validate(NotificationDefinition definition)
        {
            if (!definition.OnGrab && !definition.OnDownload) return;
            base.Validate(definition);
        }
    }
}