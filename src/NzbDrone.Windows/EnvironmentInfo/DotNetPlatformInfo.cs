using System;
using Microsoft.Win32;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Windows.EnvironmentInfo
{
    public class DotNetPlatformInfo : PlatformInfo
    {
        private readonly Logger _logger;

        public DotNetPlatformInfo(Logger logger)
        {
            _logger = logger;
            var version = GetFrameworkVersion();
            Environment.SetEnvironmentVariable("RUNTIME_VERSION", version.ToString());
            Version = version;
        }

        public override Version Version { get; }

        private Version GetFrameworkVersion()
        {
            try
            {
                const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
                {
                    if (ndpKey == null)
                    {
                        return new Version(4, 0);
                    }

                    var releaseKey = (int)ndpKey.GetValue("Release");

                    if (releaseKey >= 394802)
                    {
                        return new Version(4, 6, 2);
                    }
                    if (releaseKey >= 394254)
                    {
                        return new Version(4, 6, 1);
                    }
                    if (releaseKey >= 393295)
                    {
                        return new Version(4, 6);
                    }
                    if (releaseKey >= 379893)
                    {
                        return new Version(4, 5, 2);
                    }
                    if (releaseKey >= 378675)
                    {
                        return new Version(4, 5, 1);
                    }
                    if (releaseKey >= 378389)
                    {
                        return new Version(4, 5);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldnt get .NET framework version");
            }

            return new Version(4, 0);
        }
    }
}
