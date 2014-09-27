using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.System.Tasks
{
    public class TaskResource : RestResource
    {
        public String Name { get; set; }
        public String TaskName { get; set; }
        public Int32 Interval { get; set; }
        public DateTime LastExecution { get; set; }
        public DateTime NextExecution { get; set; }
    }
}
