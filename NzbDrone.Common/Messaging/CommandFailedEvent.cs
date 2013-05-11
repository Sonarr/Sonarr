using System;

namespace NzbDrone.Common.Messaging
{
    public class CommandFailedEvent : IEvent
    {
        public ICommand Command { get; private set; }
        public Exception Exception { get; private set; }

        public CommandFailedEvent(ICommand command, Exception exception)
        {
            Command = command;
            Exception = exception;
        }
    }
}