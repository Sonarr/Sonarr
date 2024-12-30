using Workarr.Messaging;
using Workarr.Messaging.Commands;

namespace Workarr.ProgressMessaging
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
