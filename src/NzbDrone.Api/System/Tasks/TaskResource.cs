using System;
using Sonarr.Http.REST;

namespace NzbDrone.Api.System.Tasks
{
    public class TaskResource : RestResource
    {
        public string Name { get; set; }
        public string TaskName { get; set; }
        public int Interval { get; set; }
        public DateTime LastExecution { get; set; }
        public DateTime NextExecution { get; set; }
    }
}
