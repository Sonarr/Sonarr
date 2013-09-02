using System;

namespace NzbDrone.Common.Messaging.Tracking
{
    public class TrackedCommand
    {
        public String Id { get; private set; }
        public String Name { get; private set; }
        public String Type { get; private set; }
        public ICommand Command { get; private set; }
        public ProcessState State { get; set; }
        public DateTime StateChangeTime { get; set; }
        public TimeSpan Runtime { get; set; }
        public Exception Exception { get; set; }

        public TrackedCommand()
        {
        }

        public TrackedCommand(ICommand command, ProcessState state)
        {
            Id = command.CommandId;
            Name = command.GetType().Name;
            Type = command.GetType().FullName;
            Command = command;
            State = state;
            StateChangeTime = DateTime.UtcNow;
        }
    }
}
