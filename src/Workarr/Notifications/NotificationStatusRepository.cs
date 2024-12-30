using Workarr.Datastore;
using Workarr.Messaging.Events;
using Workarr.ThingiProvider.Status;

namespace Workarr.Notifications
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
