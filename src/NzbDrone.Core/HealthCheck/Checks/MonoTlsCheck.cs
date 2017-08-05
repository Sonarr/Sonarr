using System;
using System.Linq;
using System.Reflection;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MonoTlsCheck : HealthCheckBase
    {
        private readonly IPlatformInfo _platformInfo;
        private readonly Logger _logger;

        public MonoTlsCheck(IPlatformInfo platformInfo, Logger logger)
        {
            _platformInfo = platformInfo;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (!PlatformInfo.IsMono)
            {
                return new HealthCheck(GetType());
            }

            var monoVersion = _platformInfo.Version;

            if (monoVersion >= new Version("5.0.0") && Environment.GetEnvironmentVariable("MONO_TLS_PROVIDER") == "legacy")
            {
                // Mono 5.0 still has issues in combination with libmediainfo, so disabling this check for now.
                //_logger.Debug("Mono version 5.0.0 or higher and legacy TLS provider is selected, recommending user to switch to btls.");
                //return new HealthCheck(GetType(), HealthCheckResult.Warning, "Sonarr now supports Mono 5.x with btls enabled, consider removing MONO_TLS_PROVIDER=legacy option");
            }

            return new HealthCheck(GetType());
        }

        public override bool CheckOnSchedule => false;
    }
}
