using System;

namespace NzbDrone.Common.EnvironmentInfo
{
    public static class OsInfo
    {

        public static Version Version
        {
            get
            {
                OperatingSystem os = Environment.OSVersion;
                Version version = os.Version;

                return version;
            }
        }

        public static bool IsMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
    }
}