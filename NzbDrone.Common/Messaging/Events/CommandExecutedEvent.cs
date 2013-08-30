namespace NzbDrone.Common.Messaging.Events
{
    public class CommandExecutedEvent : IEvent
    {
        public ICommand Command { get; private set; }

        public CommandExecutedEvent(ICommand command)
        {
            Command = command;
        }
    }
}