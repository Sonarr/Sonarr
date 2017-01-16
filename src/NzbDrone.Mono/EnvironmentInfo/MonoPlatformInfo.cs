using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Mono.EnvironmentInfo
{
    public class MonoPlatformInfo : PlatformInfo
    {
        private static readonly Regex VersionRegex = new Regex(@"(?<=\W|^)(?<version>\d+\.\d+(\.\d+)?(\.\d+)?)(?=\W)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public override Version Version { get; }

        public MonoPlatformInfo(Logger logger)
        {
            var runTimeVersion = new Version();

            try
            {
                var type = Type.GetType("Mono.Runtime");

                if (type != null)
                {
                    var displayNameMethod = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                    if (displayNameMethod != null)
                    {
                        var displayName = displayNameMethod.Invoke(null, null).ToString();
                        var versionMatch = VersionRegex.Match(displayName);

                        if (versionMatch.Success)
                        {
                            runTimeVersion = new Version(versionMatch.Groups["version"].Value);
                            Environment.SetEnvironmentVariable("RUNTIME_VERSION", runTimeVersion.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get mono version");
            }

            Version = runTimeVersion;
        }
    }
}
