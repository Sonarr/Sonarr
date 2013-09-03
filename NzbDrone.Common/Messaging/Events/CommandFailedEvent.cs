using System;
using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Common.Messaging.Events
{
    public class CommandFailedEvent : IEvent
    {
        public TrackedCommand TrackedCommand { get; private set; }
        public Exception Exception { get; private set; }

        public CommandFailedEvent(TrackedCommand trackedCommand, Exception exception)
        {
            TrackedCommand = trackedCommand;
            Exception = exception;
        }
    }
}