using System;
using System.Collections.Generic;
using System.Text;

namespace NzbDrone.Core.Model
{
    public class JobQueueItem : IEquatable<JobQueueItem>
    {
        public Type JobType { get; set; }
        public int TargetId { get; set; }
        public int SecondaryTargetId { get; set; }

        public bool Equals(JobQueueItem other)
        {
            if (JobType == other.JobType && TargetId == other.TargetId
                && SecondaryTargetId == other.SecondaryTargetId)
            {
                return true;
            }

            return false;
        }
    }
}
