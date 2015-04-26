using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.ProgressMessaging
{
    public class CommandUpdatedEvent : IEvent
    {
        public CommandModel Command { get; set; }

        public CommandUpdatedEvent(CommandModel command)
        {
            Command = command;
        }
    }
}
