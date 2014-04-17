using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.HealthCheck
{
    public class HealthCheck : ModelBase
    {
        public Type Source { get; set; }
        public HealthCheckResult Type { get; set; }
        public String Message { get; set; }

        public HealthCheck(Type source)
        {
            Source = source;
            Type = HealthCheckResult.Ok;
        }

        public HealthCheck(Type source, HealthCheckResult type, string message)
        {
            Source = source;
            Type = type;
            Message = message;
        }
    }

    public enum HealthCheckResult
    {
        Ok = 0,
        Warning = 1,
        Error = 2
    }
}
