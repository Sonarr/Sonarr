using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Lifecycle
{
    [LifecycleEvent]
    public class ApplicationShutdownRequested : IEvent
    {
        public bool Restarting { get; }

        public ApplicationShutdownRequested(bool restarting = false)
        {
            Restarting = restarting;
        }
    }
}
