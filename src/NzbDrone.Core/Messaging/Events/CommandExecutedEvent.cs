using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Messaging.Events
{
    public class CommandExecutedEvent : IEvent
    {
        public CommandModel Command { get; private set; }

        public CommandExecutedEvent(CommandModel command)
        {
            Command = command;
        }
    }
}
