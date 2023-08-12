using NzbDrone.Core.Notifications;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class FixFutureNotificationStatusTimes : FixFutureProviderStatusTimes<NotificationStatus>, IHousekeepingTask
    {
        public FixFutureNotificationStatusTimes(INotificationStatusRepository notificationStatusRepository)
            : base(notificationStatusRepository)
        {
        }
    }
}
