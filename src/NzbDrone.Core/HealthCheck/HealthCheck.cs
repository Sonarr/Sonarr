using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.HealthCheck
{
    public class HealthCheck : ModelBase
    {
        public HealthCheckResultType Type { get; set; }
        public String Message { get; set; }

        public HealthCheck(HealthCheckResultType type, string message)
        {
            Type = type;
            Message = message;
        }
    }

    public enum HealthCheckResultType
    {
        Warning = 1,
        Error = 2
    }
}
