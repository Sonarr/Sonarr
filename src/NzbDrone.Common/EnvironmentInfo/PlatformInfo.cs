using System;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IPlatformInfo
    {
        Version Version { get; }
    }

    public class PlatformInfo : IPlatformInfo
    {
        private static Version _version;

        static PlatformInfo()
        {
            _version = Environment.Version;
        }

        public static string PlatformName
        {
            get
            {
                return ".NET";
            }
        }

        public Version Version => _version;

        public static Version GetVersion()
        {
            return _version;
        }
    }
}
