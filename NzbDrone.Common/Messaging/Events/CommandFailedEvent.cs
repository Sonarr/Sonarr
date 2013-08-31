using System;
using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Common.Messaging.Events
{
    public class CommandFailedEvent : IEvent
    {
        public TrackedCommand Command { get; private set; }
        public Exception Exception { get; private set; }

        public CommandFailedEvent(TrackedCommand command, Exception exception)
        {
            Command = command;
            Exception = exception;
        }
    }
}