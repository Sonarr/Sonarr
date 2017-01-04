using System;
using System.Linq;
using System.Reflection;
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

            if (monoVersion == new Version("3.4.0") && HasMonoBug18599())
            {
                _logger.Debug("Mono version 3.4.0, checking for Mono bug #18599 returned positive.");
                return new HealthCheck(GetType(), HealthCheckResult.Error, "You are running an old and unsupported version of Mono with a known bug. You should upgrade to a higher version");
            }

            if (monoVersion == new Version("4.4.0") || monoVersion == new Version("4.4.1"))
            {
                _logger.Debug("Mono version {0}", monoVersion);
                return new HealthCheck(GetType(), HealthCheckResult.Error, $"Your Mono version {monoVersion} has a bug that causes issues connecting to indexers/download clients.  You should upgrade to a higher version");
            }

            if (monoVersion >= new Version("3.10"))
            {
                _logger.Debug("Mono version is 3.10 or better: {0}", monoVersion);
                return new HealthCheck(GetType());
            }

            return new HealthCheck(GetType(), HealthCheckResult.Warning, "You are running an old and unsupported version of Mono. Please upgrade Mono for improved stability.");
        }

        public override bool CheckOnConfigChange => false;

        public override bool CheckOnSchedule => false;

        private bool HasMonoBug18599()
        {
            _logger.Debug("mono version 3.4.0, checking for mono bug #18599.");
            var numberFormatterType = Type.GetType("System.NumberFormatter");

            if (numberFormatterType == null)
            {
                _logger.Debug("Couldn't find System.NumberFormatter. Aborting test.");
                return false;
            }

            var fieldInfo = numberFormatterType.GetField("userFormatProvider",
                BindingFlags.Static | BindingFlags.NonPublic);

            if (fieldInfo == null)
            {
                _logger.Debug("userFormatProvider field not found, version likely preceeds the official v3.4.0.");
                return false;
            }

            if (fieldInfo.GetCustomAttributes(false).Any(v => v is ThreadStaticAttribute))
            {
                _logger.Debug("userFormatProvider field doesn't contain the ThreadStatic Attribute, version is affected by the critical bug #18599.");
                return true;
            }

            return false;
        }
    }
}