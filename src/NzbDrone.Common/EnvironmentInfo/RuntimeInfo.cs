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

            //Guarded to avoid issues when running in a non-managed process
            var entry = Assembly.GetEntryAssembly();

            if (entry != null)
            {
                ExecutingApplication = entry.Location;
                IsWindowsTray = OsInfo.IsWindows && entry.ManifestModule.Name == $"{ProcessProvider.SONARR_PROCESS_NAME}.exe";
            }
        }

        static RuntimeInfo()
        {
            IsProduction = InternalIsProduction();
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

        public static bool IsProduction { get; }

        private static bool InternalIsProduction()
        {
            if (BuildInfo.IsDebug || Debugger.IsAttached) return false;

            //Official builds will never have such a high revision
            if (BuildInfo.Version.Revision > 10000) return false;

            try
            {
                var lowerProcessName = Process.GetCurrentProcess().ProcessName.ToLower();

                if (lowerProcessName.Contains("vshost")) return false;
                if (lowerProcessName.Contains("nunit")) return false;
                if (lowerProcessName.Contains("jetbrain")) return false;
                if (lowerProcessName.Contains("resharper")) return false;
            }
            catch
            {

            }

            try
            {
                var currentAssemblyLocation = typeof(RuntimeInfo).Assembly.Location;
                if (currentAssemblyLocation.ToLower().Contains("_output")) return false;
            }
            catch
            {

            }

            var lowerCurrentDir = Directory.GetCurrentDirectory().ToLower();
            if (lowerCurrentDir.Contains("teamcity")) return false;
            if (lowerCurrentDir.Contains("buildagent")) return false;
            if (lowerCurrentDir.Contains("_output")) return false;

            return true;
        }

        public bool IsWindowsTray { get; private set; }
    }
}
