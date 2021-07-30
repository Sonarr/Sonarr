using System;
using Microsoft.Win32;

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

        private static Version GetDotNetVersion()
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

                    if (releaseKey >= 528040)
                    {
                        return new Version(4, 8, 0);
                    }
                    if (releaseKey >= 461808)
                    {
                        return new Version(4, 7, 2);
                    }
                    if (releaseKey >= 461308)
                    {
                        return new Version(4, 7, 1);
                    }
                    if (releaseKey >= 460798)
                    {
                        return new Version(4, 7);
                    }
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
            catch (Exception ex)
            {
                Console.WriteLine("Couldnt get .NET framework version: " + ex.ToString());
            }

            return new Version(4, 0);
        }
    }
}
