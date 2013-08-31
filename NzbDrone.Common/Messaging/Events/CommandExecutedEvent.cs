using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Common.Messaging.Events
{
    public class CommandExecutedEvent : IEvent
    {
        public TrackedCommand Command { get; private set; }

        public CommandExecutedEvent(TrackedCommand command)
        {
            Command = command;
        }
    }
}