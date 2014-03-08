using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.HealthCheck;


namespace NzbDrone.Api.Health
{
    public class HealthResource : RestResource
    {
        public HealthCheckResultType Type { get; set; }
        public String Message { get; set; }
    }
}
