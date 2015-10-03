using System;
using Newtonsoft.Json;
using NzbDrone.Api.REST;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Api.Commands
{
    public class CommandResource : RestResource
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public Command Body { get; set; }
        public CommandPriority Priority { get; set; }
        public CommandStatus Status { get; set; }
        public DateTime Queued { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Exception { get; set; }
        public CommandTrigger Trigger { get; set; }

        [JsonIgnore]
        public string CompletionMessage { get; set; }

        //Legacy
        public CommandStatus State
        {
            get
            {
                return Status;
            }

            set { }
        }

        public bool Manual
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

        public bool SendUpdatesToClient
        {
            get
            {
                if (Body != null) return Body.SendUpdatesToClient;

                return false;
            }

            set { }
        }

        public bool UpdateScheduledTask
        {
            get
            {
                if (Body != null) return Body.UpdateScheduledTask;

                return false;
            }

            set { }
        }

        public DateTime? LastExecutionTime { get; set; }
    }
}
