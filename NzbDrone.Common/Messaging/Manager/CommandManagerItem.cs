using System;

namespace NzbDrone.Common.Messaging.Manager
{
    public class CommandManagerItem
    {
        public String Type { get; set; }
        public ICommand Command { get; set; }
        public CommandState State { get; set; }
        
        public CommandManagerItem()
        {
        }

        public CommandManagerItem(ICommand command, CommandState state)
        {
            Type = command.GetType().FullName;
            Command = command;
            State = state;
        }
    }

    public enum CommandState
    {
        Running = 0,
        Completed = 1,
        Failed = 2
    }
}
