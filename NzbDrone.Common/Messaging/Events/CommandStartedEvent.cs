namespace NzbDrone.Common.Messaging.Events
{
    public class CommandStartedEvent : IEvent
    {
        public ICommand Command { get; private set; }

        public CommandStartedEvent(ICommand command)
        {
            Command = command;
        }
    }
}