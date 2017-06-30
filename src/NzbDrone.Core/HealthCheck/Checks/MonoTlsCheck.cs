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
                _logger.Debug("Mono version 5.0.0 or higher and legacy TLS provider is selected, recommending user to switch to btls.");
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Sonarr now supports Mono 5.x with btls enabled, consider removing MONO_TLS_PROVIDER=legacy option");
            }

            return new HealthCheck(GetType());
        }

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
