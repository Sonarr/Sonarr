using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.HealthCheck
{
    public class HealthCheckFailedEvent : IEvent
    {
        public HealthCheck HealthCheck { get; private set; }
        public bool IsInStartupGracePeriod { get; private set; }

        public HealthCheckFailedEvent(HealthCheck healthCheck, bool isInStartupGracePeriod)
        {
            HealthCheck = healthCheck;
            IsInStartupGracePeriod = isInStartupGracePeriod;
        }
    }
}
