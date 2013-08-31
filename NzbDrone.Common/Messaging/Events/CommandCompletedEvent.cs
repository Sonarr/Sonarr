using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Common.Messaging.Events
{
    public class CommandCompletedEvent : IEvent
    {
        public TrackedCommand Command { get; private set; }

        public CommandCompletedEvent(TrackedCommand command)
        {
            Command = command;
        }
    }
}