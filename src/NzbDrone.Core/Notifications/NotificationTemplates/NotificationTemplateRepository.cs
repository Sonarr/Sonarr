using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Notifications.NotificationTemplates
{
    public interface INotificationTemplateRepository : IBasicRepository<NotificationTemplate>
    {
    }

    public class NotificationTemplateRepository : BasicRepository<NotificationTemplate>, INotificationTemplateRepository
    {
        public NotificationTemplateRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
