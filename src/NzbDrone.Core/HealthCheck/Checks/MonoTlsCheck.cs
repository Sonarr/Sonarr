using System;
using System.Linq;
using System.Reflection;
using NLog;
using NLog.Fluent;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation.Extensions;

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

            if (monoVersion >= new Version("5.8.0") && Environment.GetEnvironmentVariable("MONO_TLS_PROVIDER") == "legacy")
            {
                _logger.Debug()
                       .Message("Mono version {0} and legacy TLS provider is selected, recommending user to switch to btls.", monoVersion)
                       .WriteSentryDebug("LegacyTlsProvider", monoVersion.ToString())
                       .Write();

                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Sonarr Mono 4.x tls workaround still enabled, consider removing MONO_TLS_PROVIDER=legacy environment option");
            }

            return new HealthCheck(GetType());
        }

        public override bool CheckOnSchedule => false;
    }
}
