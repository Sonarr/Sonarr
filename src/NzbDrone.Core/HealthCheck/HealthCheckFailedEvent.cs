using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.HealthCheck
{
    public class HealthCheckFailedEvent : IEvent
    {
        public HealthCheck HealthCheck { get; private set; }
        public bool IsInStartupGraceperiod { get; private set; }

        public HealthCheckFailedEvent(HealthCheck healthCheck, bool isInStartupGraceperiod)
        {
            HealthCheck = healthCheck;
            IsInStartupGraceperiod = isInStartupGraceperiod;
        }
    }
}
