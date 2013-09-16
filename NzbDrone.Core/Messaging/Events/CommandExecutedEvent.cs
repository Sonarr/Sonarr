using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Messaging.Events
{
    public class CommandExecutedEvent : IEvent
    {
        public Command Command { get; private set; }

        public CommandExecutedEvent(Command trackedCommand)
        {
            Command = trackedCommand;
        }
    }
}