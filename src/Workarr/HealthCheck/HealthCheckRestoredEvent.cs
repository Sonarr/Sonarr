using Workarr.Messaging;

namespace Workarr.HealthCheck
{
    public class HealthCheckRestoredEvent : IEvent
    {
        public HealthCheck PreviousCheck { get; private set; }
        public bool IsInStartupGracePeriod { get; private set; }

        public HealthCheckRestoredEvent(HealthCheck previousCheck, bool isInStartupGracePeriod)
        {
            PreviousCheck = previousCheck;
            IsInStartupGracePeriod = isInStartupGracePeriod;
        }
    }
}
