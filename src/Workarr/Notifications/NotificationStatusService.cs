using NLog;
using Workarr.EnvironmentInfo;
using Workarr.Messaging.Events;
using Workarr.ThingiProvider.Status;

namespace Workarr.Notifications
{
    public interface INotificationStatusService : IProviderStatusServiceBase<NotificationStatus>
    {
    }

    public class NotificationStatusService : ProviderStatusServiceBase<INotification, NotificationStatus>, INotificationStatusService
    {
        public NotificationStatusService(INotificationStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
            MinimumTimeSinceInitialFailure = TimeSpan.FromMinutes(5);
            MaximumEscalationLevel = 5;
        }
    }
}
