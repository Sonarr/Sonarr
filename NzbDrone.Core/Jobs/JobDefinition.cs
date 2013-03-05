using System;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Jobs
{
    public class JobDefinition : ModelBase
    {
        public Boolean Enable { get; set; }

        public String TypeName { get; set; }

        public String Name { get; set; }

        public Int32 Interval { get; set; }

        public DateTime LastExecution { get; set; }

        public Boolean Success { get; set; }
    }
}