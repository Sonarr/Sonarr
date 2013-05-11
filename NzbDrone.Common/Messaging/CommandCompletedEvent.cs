namespace NzbDrone.Common.Messaging
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