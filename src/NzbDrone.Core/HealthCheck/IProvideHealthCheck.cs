using System;

namespace NzbDrone.Core.HealthCheck
{
    public interface IProvideHealthCheck
    {
        HealthCheck Check();
        Boolean CheckOnStartup { get; }
        Boolean CheckOnConfigChange { get; }
        Boolean CheckOnSchedule { get; }
    }
}
