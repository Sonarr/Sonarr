using System;

namespace NzbDrone.Common.Messaging.Tracking
{
    public class TrackedCommand
    {
        public String Id { get; private set; }
        public String Name { get; private set; }
        public String Type { get; private set; }
        public ICommand Command { get; private set; }
        public CommandState State { get; set; }
        public DateTime StateChangeTime { get; set; }
        public TimeSpan Runtime { get; set; }
        public Exception Exception { get; set; }
        
        public TrackedCommand(ICommand command, CommandState state)
        {
            Id = command.CommandId;
            Name = command.GetType().Name;
            Type = command.GetType().FullName;
            Command = command;
            State = state;
            StateChangeTime = DateTime.UtcNow;
        }
    }

    public enum CommandState
    {
        Running = 0,
        Completed = 1,
        Failed = 2
    }
}
