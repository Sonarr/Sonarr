using System;

namespace NzbDrone.Common.EnvironmentInfo
{
    public static class OsInfo
    {

        static OsInfo()
        {
            Version = Environment.OSVersion.Version;
            IsMono = Type.GetType("Mono.Runtime") != null;

            int platform = (int)Environment.OSVersion.Platform;
            IsLinux = (platform == 4) || (platform == 6) || (platform == 128);
            
        }

        public static Version Version { get; private set; }

        public static bool IsMono { get; private set; }

        public static bool IsLinux { get; private set; }

        public static bool IsWindows
        {
            get
            {
                return !IsLinux;
            }
        }
    }
}