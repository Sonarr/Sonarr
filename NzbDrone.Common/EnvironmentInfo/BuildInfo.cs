using System;
using System.IO;
using System.Reflection;

namespace NzbDrone.Common.EnvironmentInfo
{
    public static class BuildInfo
    {
        public static Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public static DateTime BuildDateTime
        {
            get
            {
                var fileLocation = Assembly.GetCallingAssembly().Location;
                return new FileInfo(fileLocation).LastWriteTimeUtc;
            }
        }

        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}