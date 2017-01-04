using System;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Windows.EnvironmentInfo
{
    public class WindowsVersionInfo : IOsVersionAdapter
    {
        public bool Enabled => OsInfo.IsWindows;
        public OsVersionModel Read()
        {
            return new OsVersionModel("Windows", Environment.OSVersion.Version.ToString(), Environment.OSVersion.VersionString);
        }
    }
}