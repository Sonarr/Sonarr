using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Notifications
{
    public interface INotificationStatusRepository : IProviderStatusRepository<NotificationStatus>
    {
    }

    public class NotificationStatusRepository : ProviderStatusRepository<NotificationStatus>, INotificationStatusRepository
    {
        public NotificationStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
