namespace NzbDrone.Common.Messaging.Events
{
    public class CommandCompletedEvent : IEvent
    {
        public ICommand Command { get; private set; }

        public CommandCompletedEvent(ICommand command)
        {
            Command = command;
        }
    }
}