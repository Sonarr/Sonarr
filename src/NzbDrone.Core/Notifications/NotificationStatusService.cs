using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Notifications
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
