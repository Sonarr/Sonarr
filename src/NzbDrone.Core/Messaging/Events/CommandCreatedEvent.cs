using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Messaging.Events
{
    public class CommandCreatedEvent : IEvent
    {
        public Command Command { get; private set; }

        public CommandCreatedEvent(Command trackedCommand)
        {
            Command = trackedCommand;
        }
    }
}