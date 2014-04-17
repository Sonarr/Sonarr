using System;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MonoVersionCheck : HealthCheckBase
    {
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;
        private static readonly Regex VersionRegex = new Regex(@"(?<=\W)(?<version>\d+\.\d+\.\d+(\.\d+)?)(?=\W)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public MonoVersionCheck(IProcessProvider processProvider, Logger logger)
        {
            _processProvider = processProvider;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (!OsInfo.IsMono)
            {
                return new HealthCheck(GetType());
            }

            var output = _processProvider.StartAndCapture("mono", "--version");

            foreach (var line in output.Standard)
            {
                var versionMatch = VersionRegex.Match(line);

                if (versionMatch.Success)
                {
                    var version = new Version(versionMatch.Groups["version"].Value);

                    if (version >= new Version(3, 2))
                    {
                        _logger.Debug("mono version is 3.2 or better: {0}", version.ToString());
                        return new HealthCheck(GetType());
                    }
                }
            }

            return new HealthCheck(GetType(), HealthCheckResult.Warning, "mono version is less than 3.2, upgrade for improved stability");
        }

        public override bool CheckOnConfigChange
        {
            get
            {
                return false;
            }
        }

        public override bool CheckOnSchedule
        {
            get
            {
                return false;
            }
        }
    }
}
