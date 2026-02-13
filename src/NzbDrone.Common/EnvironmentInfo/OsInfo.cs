using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace NzbDrone.Common.EnvironmentInfo
{
    public class OsInfo : IOsInfo
    {
        public static Os Os { get; }

        public static bool IsNotWindows => !IsWindows;
        public static bool IsLinux => Os == Os.Linux || Os == Os.LinuxMusl || Os == Os.Bsd;
        public static bool IsOsx => Os == Os.Osx;
        public static bool IsWindows => Os == Os.Windows;

        // this needs to not be static so we can mock it
        public bool IsDocker { get; }
        public bool IsPodman { get; }
        public bool IsContainerized { get; }

        public string Version { get; }
        public string Name { get; }
        public string FullName { get; }

        static OsInfo()
        {
            if (OperatingSystem.IsWindows())
            {
                Os = Os.Windows;
            }
            else if (OperatingSystem.IsMacOS())
            {
                Os = Os.Osx;
            }
            else if (OperatingSystem.IsFreeBSD())
            {
                Os = Os.Bsd;
            }
            else
            {
#if ISMUSL
                Os = Os.LinuxMusl;
#else
                Os = Os.Linux;
#endif
            }
        }

        public OsInfo(IEnumerable<IOsVersionAdapter> versionAdapters, Logger logger)
        {
            OsVersionModel osInfo = null;

            foreach (var osVersionAdapter in versionAdapters.Where(c => c.Enabled))
            {
                try
                {
                    osInfo = osVersionAdapter.Read();
                }
                catch (Exception e)
                {
                    logger.Error(e, "Couldn't get OS Version info");
                }

                if (osInfo != null)
                {
                    break;
                }
            }

            if (osInfo != null)
            {
                Name = osInfo.Name;
                Version = osInfo.Version;
                FullName = osInfo.FullName;
            }
            else
            {
                Name = Os.ToString();
                FullName = Name;
            }

            if (IsLinux)
            {
                IsDocker = File.Exists("/.dockerenv") ||
                           (File.Exists("/proc/1/cgroup") && File.ReadAllText("/proc/1/cgroup").Contains("/docker/"));
                IsPodman = File.Exists("/run/.containerenv") ||
                           Environment.GetEnvironmentVariable("container") != null;

                IsContainerized = IsDocker || IsPodman || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") is "true" or "1";
            }
        }
    }

    public interface IOsInfo
    {
        string Version { get; }
        string Name { get; }
        string FullName { get; }
        bool IsDocker { get; }
        bool IsPodman { get; }
        bool IsContainerized { get; }
    }

    public enum Os
    {
        Windows,
        Linux,
        Osx,
        LinuxMusl,
        Bsd
    }
}
