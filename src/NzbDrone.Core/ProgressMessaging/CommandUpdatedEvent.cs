using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.ProgressMessaging
{
    public class CommandUpdatedEvent : IEvent
    {
        public Command Command { get; set; }

        public CommandUpdatedEvent(Command command)
        {
            Command = command;
        }
    }
}
