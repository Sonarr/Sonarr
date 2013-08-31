using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Common.Messaging.Events
{
    public class CommandStartedEvent : IEvent
    {
        public TrackedCommand Command { get; private set; }

        public CommandStartedEvent(TrackedCommand command)
        {
            Command = command;
        }
    }
}