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

            // Known buggy Mono versions
            if (monoVersion == new Version("4.4.0") || monoVersion == new Version("4.4.1"))
            {
                _logger.Debug("Mono version {0}", monoVersion);
                return new HealthCheck(GetType(), HealthCheckResult.Error,
                    $"Currently installed Mono version {monoVersion} has a bug that causes issues connecting to indexers/download clients. You should upgrade to a higher version",
                    "#currently_installed_mono_version-is_old_and_unsupported");
            }

            // Currently best stable Mono version (5.18 gets us .net 4.7.2 support)
            var bestVersion = new Version("5.20");
            var targetVersion = new Version("5.18");
            if (monoVersion >= targetVersion)
            {
                _logger.Debug("Mono version is {0} or better: {1}", targetVersion, monoVersion);
                return new HealthCheck(GetType());
            }

            // Stable Mono versions
            var stableVersion = new Version("5.18");
            if (monoVersion >= stableVersion)
            {
                _logger.Debug("Mono version is {0} or better: {1}", stableVersion, monoVersion);
                return new HealthCheck(GetType(), HealthCheckResult.Notice,
                    $"Currently installed Mono version {monoVersion} is supported but upgrading to {bestVersion} is recommended.",
                    "#currently_installed_mono_version_is_supported_but_upgrading_is_recommended");
            }
            
            var oldVersion = new Version("5.4");
            if (monoVersion >= oldVersion)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error,
                $"Currently installed Mono version {monoVersion} is no longer supported. Please upgrade Mono to version {bestVersion}.",
                "#currently_installed_mono_version-is_old_and_unsupported");
            }

            return new HealthCheck(GetType(), HealthCheckResult.Error, 
                $"Currently installed Mono version {monoVersion} is old and unsupported. Please upgrade Mono to version {bestVersion}.", 
                "#currently_installed_mono_version-is_old_and_unsupported");
        }

        public override bool CheckOnSchedule => false;
    }
}
