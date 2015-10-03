using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.HealthCheck;

namespace NzbDrone.Api.Health
{
    public class HealthResource : RestResource
    {
        public HealthCheckResult Type { get; set; }
        public string Message { get; set; }
        public Uri WikiUrl { get; set; }
    }
}
