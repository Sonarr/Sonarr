using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MonoVersionCheck : HealthCheckBase
    {
        private readonly IPlatformInfo _platformInfo;
        private readonly Logger _logger;

        public MonoVersionCheck(IPlatformInfo platformInfo, Logger logger)
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

            if (monoVersion == new Version("4.4.0") || monoVersion == new Version("4.4.1"))
            {
                _logger.Debug("Mono version {0}", monoVersion);
                return new HealthCheck(GetType(), HealthCheckResult.Error, $"Your Mono version {monoVersion} has a bug that causes issues connecting to indexers/download clients. You should upgrade to a higher version");
            }

            if (monoVersion >= new Version("4.4.2"))
            {
                _logger.Debug("Mono version is 4.4.2 or better: {0}", monoVersion);
                return new HealthCheck(GetType());
            }

            return new HealthCheck(GetType(), HealthCheckResult.Warning, "You are running an old and unsupported version of Mono. Please upgrade Mono for improved stability.");
        }

        public override bool CheckOnSchedule => false;
    }
}
