using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.HealthCheck
{
    public class HealthCheckFailedEvent : IEvent
    {
        public HealthCheck HealthCheck;

        public HealthCheckFailedEvent(HealthCheck healthCheck)
        {
            HealthCheck = healthCheck;
        }
    }
}
