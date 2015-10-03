using System;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Http
{
    public static class UserAgentBuilder
    {
        public static string UserAgent { get; private set; }

        static UserAgentBuilder()
        {
            UserAgent = string.Format("Sonarr/{0} ({1} {2}) ",
                BuildInfo.Version,
                OsInfo.Os, OsInfo.Version.ToString(2));
        }
    }
}