using System;

namespace NzbDrone.Core.Messaging.Commands
{
    public abstract class Command
    {
        public virtual bool SendUpdatesToClient => false;

        public virtual bool UpdateScheduledTask => true;

        public virtual string CompletionMessage => "Completed";

        public string Name { get; private set; }
        public DateTime? LastExecutionTime { get; set; }
        public CommandTrigger Trigger { get; set; }

        public Command()
        {
            Name = GetType().Name.Replace("Command", "");
        }
    }
}
