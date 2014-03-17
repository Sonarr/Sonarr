using System;
using System.Runtime.InteropServices;

namespace NzbDrone.Common.EnvironmentInfo
{
    public static class OsInfo
    {

        static OsInfo()
        {
            var platform = (int)Environment.OSVersion.Platform;

            Version = Environment.OSVersion.Version;

            IsMonoRuntime = Type.GetType("Mono.Runtime") != null;           
            IsMono = (platform == 4) || (platform == 6) || (platform == 128);
            IsOsx = IsRunningOnMac();
            IsLinux = IsMono && !IsOsx;
            IsWindows = !IsMono;

            FirstDayOfWeek = DateTime.Today.GetFirstDayOfWeek().DayOfWeek;

            if (!IsMono)
            {
                Os = Os.Windows;
            }

            else
            {
                Os = IsOsx ? Os.Osx : Os.Linux;
            }
        }

        public static Version Version { get; private set; }
        public static bool IsMonoRuntime { get; private set; }
        public static bool IsMono { get; private set; }
        public static bool IsLinux { get; private set; }
        public static bool IsOsx { get; private set; }
        public static bool IsWindows { get; private set; }
        public static Os Os { get; private set; }
        public static DayOfWeek FirstDayOfWeek { get; private set; }

        //Borrowed from: https://github.com/jpobst/Pinta/blob/master/Pinta.Core/Managers/SystemManager.cs
        //From Managed.Windows.Forms/XplatUI
        [DllImport("libc")]
        static extern int uname(IntPtr buf);

        static bool IsRunningOnMac()
        {
            var buf = IntPtr.Zero;

            try
            {
                buf = Marshal.AllocHGlobal(8192);
                // This is a hacktastic way of getting sysname from uname ()
                if (uname(buf) == 0)
                {
                    var os = Marshal.PtrToStringAnsi(buf);

                    if (os == "Darwin")
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (buf != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buf);
                }
            }

            return false;
        }
    }

    public enum Os
    {
        Windows,
        Linux,
        Osx
    }
}