using System;

namespace NzbDrone.Core.Model
{
    public class JobQueueItem : IEquatable<JobQueueItem>
    {
        public Type JobType { get; set; }
        public int TargetId { get; set; }
        public int SecondaryTargetId { get; set; }

        public bool Equals(JobQueueItem other)
        {
            return (JobType == other.JobType && TargetId == other.TargetId
                && SecondaryTargetId == other.SecondaryTargetId);
        }

        public override string ToString()
        {
            return string.Format("[{0}({1}, {2})]", JobType.Name, TargetId, SecondaryTargetId);
        }
    }
}
