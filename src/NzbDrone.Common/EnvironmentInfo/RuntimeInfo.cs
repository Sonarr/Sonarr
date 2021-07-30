using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using NLog;
using NzbDrone.Common.Processes;

namespace NzbDrone.Common.EnvironmentInfo
{
    public class RuntimeInfo : IRuntimeInfo
    {
        private readonly Logger _logger;
        private readonly DateTime _startTime = DateTime.UtcNow;

        public RuntimeInfo(IServiceProvider serviceProvider, Logger logger)
        {
            _logger = logger;

            IsWindowsService = !IsUserInteractive &&
                               OsInfo.IsWindows &&
                               serviceProvider.ServiceExist(ServiceProvider.SERVICE_NAME) &&
                               serviceProvider.GetStatus(ServiceProvider.SERVICE_NAME) == ServiceControllerStatus.StartPending;

            // net6.0 will return Sonarr.dll for entry assembly, we need the actual
            // executable name (Sonarr on linux).  On mono this will return the location of
            // the mono executable itself, which is not what we want.
            var entry = Process.GetCurrentProcess().MainModule;

            if (entry != null)
            {
                ExecutingApplication = entry.FileName;
                IsWindowsTray = OsInfo.IsWindows && entry.ModuleName == $"{ProcessProvider.SONARR_PROCESS_NAME}.exe";
            }
        }

        static RuntimeInfo()
        {
            var officialBuild = InternalIsOfficialBuild();

            // An build running inside of the testing environment. (Analytics disabled)
            IsTesting = InternalIsTesting();

            // An official build running outside of the testing environment. (Analytics configurable)
            IsProduction = !IsTesting && officialBuild;

            // An unofficial build running outside of the testing environment. (Analytics enabled)
            IsDevelopment = !IsTesting && !officialBuild && !InternalIsDebug();
        }

        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
        }

        public static bool IsUserInteractive => Environment.UserInteractive;

        bool IRuntimeInfo.IsUserInteractive => IsUserInteractive;

        public bool IsAdmin
        {
            get
            {
                if (OsInfo.IsNotWindows)
                {
                    return false;
                }

                try
                {
                    var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Error checking if the current user is an administrator.");
                    return false;
                }
            }
        }

        public bool IsWindowsService { get; private set; }

        public bool IsExiting { get; set; }
        public bool IsTray
        {
            get
            {
                if (OsInfo.IsWindows)
                {
                    return IsUserInteractive && Process.GetCurrentProcess().ProcessName.Equals(ProcessProvider.SONARR_PROCESS_NAME, StringComparison.InvariantCultureIgnoreCase);
                }

                return false;
            }
        }

        public RuntimeMode Mode
        {
            get
            {
                if (IsWindowsService)
                {
                    return RuntimeMode.Service;
                }

                if (IsTray)
                {
                    return RuntimeMode.Tray;
                }

                return RuntimeMode.Console;
            }
        }

        public bool RestartPending { get; set; }
        public string ExecutingApplication { get; }

        public static bool IsTesting { get; }
        public static bool IsProduction { get; }
        public static bool IsDevelopment { get; }


        private static bool InternalIsTesting()
        {
            try
            {
                var lowerProcessName = Process.GetCurrentProcess().ProcessName.ToLower();

                if (lowerProcessName.Contains("vshost"))
                {
                    return true;
                }

                if (lowerProcessName.Contains("nunit"))
                {
                    return true;
                }

                if (lowerProcessName.Contains("jetbrain"))
                {
                    return true;
                }

                if (lowerProcessName.Contains("resharper"))
                {
                    return true;
                }
            }
            catch
            {

            }

            try
            {
                var currentAssemblyLocation = typeof(RuntimeInfo).Assembly.Location;
                if (currentAssemblyLocation.ToLower().Contains("_output"))
                {
                    return true;
                }

                if (currentAssemblyLocation.ToLower().Contains("_tests"))
                {
                    return true;
                }
            }
            catch
            {
            }

            var lowerCurrentDir = Directory.GetCurrentDirectory().ToLower();
            if (lowerCurrentDir.Contains("vsts"))
            {
                return true;
            }

            if (lowerCurrentDir.Contains("buildagent"))
            {
                return true;
            }

            if (lowerCurrentDir.Contains("_output"))
            {
                return true;
            }

            if (lowerCurrentDir.Contains("_tests"))
            {
                return true;
            }

            return false;
        }

        private static bool InternalIsDebug()
        {
            if (BuildInfo.IsDebug || Debugger.IsAttached)
            {
                return true;
            }

            return false;
        }

        private static bool InternalIsOfficialBuild()
        {
            //Official builds will never have such a high revision
            if (BuildInfo.Version.Major >= 10 || BuildInfo.Version.Revision > 10000)
            {
                return false;
            }

            return true;
        }

        public bool IsWindowsTray { get; private set; }
    }
}
