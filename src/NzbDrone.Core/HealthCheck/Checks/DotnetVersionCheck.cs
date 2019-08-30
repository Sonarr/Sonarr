using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class DotnetVersionCheck : HealthCheckBase
    {
        private readonly IPlatformInfo _platformInfo;
        private readonly IOsInfo _osInfo;
        private readonly Logger _logger;

        public DotnetVersionCheck(IPlatformInfo platformInfo, IOsInfo osInfo, Logger logger)
        {
            _platformInfo = platformInfo;
            _osInfo = osInfo;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (!PlatformInfo.IsDotNet)
            {
                return new HealthCheck(GetType());
            }

            var dotnetVersion = _platformInfo.Version;

            // Target .Net version, which would allow us to increase our target framework
            var targetVersion = new Version("4.7.2");
            if (Version.TryParse(_osInfo.Version, out var osVersion) && osVersion < new Version("10.0.14393"))
            {
                // Windows 10 LTSB 1511 and before do not support 4.7.x
                targetVersion = new Version("4.6.2");
            }

            if (dotnetVersion >= targetVersion)
            {
                _logger.Debug("Dotnet version is {0} or better: {1}", targetVersion, dotnetVersion);
                return new HealthCheck(GetType());
            }

            // Supported .net version but below our desired target
            var stableVersion = new Version("4.6.2");
            if (dotnetVersion >= stableVersion)
            {
                _logger.Debug("Dotnet version is {0} or better: {1}", stableVersion, dotnetVersion);
                return new HealthCheck(GetType(), HealthCheckResult.Notice,
                    $"Currently installed .Net Framework {dotnetVersion} is supported but we recommend upgrading to at least {targetVersion}.",
                    "#currently-installed-net-framework-is-supported-but-upgrading-is-recommended");
            }

            return new HealthCheck(GetType(), HealthCheckResult.Error,
                $"Currently installed .Net Framework {dotnetVersion} is old and unsupported. Please upgrade the .Net Framework to at least {targetVersion}.",
                "#currently-installed-net-framework-is-old-and-unsupported");
        }

        public override bool CheckOnSchedule => false;
    }
}
