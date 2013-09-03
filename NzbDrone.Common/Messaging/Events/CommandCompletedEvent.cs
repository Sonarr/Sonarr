using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Common.Messaging.Events
{
    public class CommandCompletedEvent : IEvent
    {
        public TrackedCommand TrackedCommand { get; private set; }

        public CommandCompletedEvent(TrackedCommand trackedCommand)
        {
            TrackedCommand = trackedCommand;
        }
    }
}