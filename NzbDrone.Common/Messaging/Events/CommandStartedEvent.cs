using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Common.Messaging.Events
{
    public class CommandStartedEvent : IEvent
    {
        public TrackedCommand TrackedCommand { get; private set; }

        public CommandStartedEvent(TrackedCommand trackedCommand)
        {
            TrackedCommand = trackedCommand;
        }
    }
}