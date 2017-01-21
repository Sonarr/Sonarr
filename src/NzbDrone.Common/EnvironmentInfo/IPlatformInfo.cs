using System;

namespace NzbDrone.Common.EnvironmentInfo
{

    public enum PlatformType
    {
        DotNet = 0,
        Mono = 1
    }

    public interface IPlatformInfo
    {
        Version Version { get; }
    }

    public abstract class PlatformInfo : IPlatformInfo
    {
        static PlatformInfo()
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                Platform = PlatformType.Mono;
            }
            else
            {
                Platform = PlatformType.DotNet;
            }
        }

        public static PlatformType Platform { get; }
        public static bool IsMono => Platform == PlatformType.Mono;
        public static bool IsDotNet => Platform == PlatformType.DotNet;

        public static string PlatformName
        {
            get
            {
                if (IsDotNet)
                {
                    return ".NET";
                }

                return "Mono";
            }
        }

        public abstract Version Version { get; }
    }
}
