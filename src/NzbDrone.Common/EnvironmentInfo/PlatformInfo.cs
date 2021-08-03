using System;

namespace NzbDrone.Common.EnvironmentInfo
{
    public enum PlatformType
    {
        DotNet = 0,
        Mono = 1,
        NetCore = 2
    }

    public interface IPlatformInfo
    {
        Version Version { get; }
    }

    public class PlatformInfo : IPlatformInfo
    {
        private static PlatformType _platform;
        private static Version _version;

        static PlatformInfo()
        {
            _platform = PlatformType.NetCore;
            _version = Environment.Version;
        }

        public static PlatformType Platform => _platform;
        public static bool IsDotNet => Platform == PlatformType.DotNet;
        public static bool IsNetCore => Platform == PlatformType.NetCore;

        public static string PlatformName
        {
            get
            {
                if (IsDotNet)
                {
                    return ".NET";
                }
                else
                {
                    return ".NET Core";
                }
            }
        }

        public Version Version => _version;

        public static Version GetVersion()
        {
            return _version;
        }
    }
}
