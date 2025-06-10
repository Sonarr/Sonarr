using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public interface INotificationRepository : IProviderRepository<NotificationDefinition>
    {
        void UpdateSettings(NotificationDefinition model);
        void removeNotificationTemplate(int notificationTemplateId);
    }

    public class NotificationRepository : ProviderRepository<NotificationDefinition>, INotificationRepository
    {
        public NotificationRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void UpdateSettings(NotificationDefinition model)
        {
            SetFields(model, m => m.Settings);
        }

        public void removeNotificationTemplate(int notificationTemplateId)
        {
            var models = All();

            foreach (var model in models)
            {
                if (model.NotificationTemplateId == notificationTemplateId)
                {
                    model.NotificationTemplateId = 0;
                    Update(model);
                }
            }
        }
    }
}
