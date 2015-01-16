using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Api.Commands
{
    public class CommandResource : RestResource
    {
        public String Name { get; set; }
        public String Message { get; set; }
        
        public Command Body { get; set; }
        public CommandPriority Priority { get; set; }
        public CommandStatus Status { get; set; }
        public DateTime Queued { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Exception { get; set; }
        public CommandTrigger Trigger { get; set; }

        //Legacy
        public CommandStatus State
        {
            get
            {
                return Status;
            }

            set { }
        }

        public Boolean Manual
        {
            get
            {
                return Trigger == CommandTrigger.Manual;
            }

            set { }
        }

        public DateTime StartedOn
        {
            get
            {
                return Queued;
            }

            set { }
        }

        public DateTime? StateChangeTime
        {
            get
            {

                if (Started.HasValue) return Started.Value;

                return Ended;
            }

            set { }
        }

        public Boolean SendUpdatesToClient
        {
            get
            {
                if (Body != null) return Body.SendUpdatesToClient;

                return false;
            }

            set { }
        }

        public DateTime? LastExecutionTime { get; set; }
    }
}
