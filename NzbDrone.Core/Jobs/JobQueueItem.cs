using System;

namespace NzbDrone.Core.Jobs
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
            if (Options != null)
            {
                return string.Format("[{0}({1})]", JobType.Name, Options);
            }

            return string.Format("[{0}]", JobType.Name);
        }

        public enum JobSourceType
        {
            User,
            Scheduler
        }
    }
}
