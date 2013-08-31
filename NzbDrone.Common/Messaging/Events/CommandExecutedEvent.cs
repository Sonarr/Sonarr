using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Common.Messaging.Events
{
    public class CommandExecutedEvent : IEvent
    {
        public TrackedCommand TrackedCommand { get; private set; }

        public CommandExecutedEvent(TrackedCommand trackedCommand)
        {
            TrackedCommand = trackedCommand;
        }
    }
}