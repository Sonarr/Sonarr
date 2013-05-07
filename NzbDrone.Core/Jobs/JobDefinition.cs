using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Jobs
{
    public class JobDefinition : ModelBase
    {
        public String Name { get; set; }
        public Int32 Interval { get; set; }
        public DateTime LastExecution { get; set; }
        public Boolean Success { get; set; }
    }
}