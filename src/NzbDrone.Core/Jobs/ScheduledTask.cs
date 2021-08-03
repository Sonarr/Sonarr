using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Jobs
{
    public class ScheduledTask : ModelBase
    {
        public string TypeName { get; set; }
        public int Interval { get; set; }
        public DateTime LastExecution { get; set; }
        public CommandPriority Priority { get; set; }

        public ScheduledTask()
        {
            Priority = CommandPriority.Low;
        }
    }
}
