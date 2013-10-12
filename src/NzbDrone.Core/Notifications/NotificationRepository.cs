using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;


namespace NzbDrone.Core.Notifications
{
    public interface INotificationRepository : IProviderRepository<NotificationDefinition>
    {

    }

    public class NotificationRepository : ProviderRepository<NotificationDefinition>, INotificationRepository
    {
        public NotificationRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}