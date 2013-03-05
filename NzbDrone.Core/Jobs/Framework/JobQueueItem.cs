using System.Linq;
using System;

namespace NzbDrone.Core.Jobs.Framework
{
    public class JobQueueItem : IEquatable<JobQueueItem>
    {
        public Type JobType { get; set; }
        public dynamic Options { get; set; }

        public JobSourceType Source { get; set; }

        public bool Equals(JobQueueItem other)
        {
            return (JobType == other.JobType && Options == other.Options);
        }

        public override string ToString()
        {
            return string.Format("[{0}({1})]", JobType.Name, Options);
        }

        public enum JobSourceType
        {
            User,
            Scheduler
        }
    }
}
