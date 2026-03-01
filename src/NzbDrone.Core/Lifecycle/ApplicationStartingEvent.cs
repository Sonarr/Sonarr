using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Lifecycle
{
    [LifecycleEvent]
    public class ApplicationStartingEvent : IEvent
    {
    }
}
