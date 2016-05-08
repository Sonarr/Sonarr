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
    public abstract class RuntimeInfoBase : IRuntimeInfo
    {
        private readonly Logger _logger;
        private readonly DateTime _startTime = DateTime.UtcNow;

        public RuntimeInfoBase(IServiceProvider serviceProvider, Logger logger)
        {
            _logger = logger;

            IsWindowsService = !IsUserInteractive &&
                               OsInfo.IsWindows &&
                               serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME) &&
                               serviceProvider.GetStatus(ServiceProvider.NZBDRONE_SERVICE_NAME) == ServiceControllerStatus.StartPending;

            //Guarded to avoid issues when running in a non-managed process 
            var entry = Assembly.GetEntryAssembly();

            if (entry != null)
            {
                ExecutingApplication = entry.Location;
            }            
        }

        static RuntimeInfoBase()
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

        public static bool IsUserInteractive
        {
            get { return Environment.UserInteractive; }
        }

        bool IRuntimeInfo.IsUserInteractive
        {
            get
            {
                return IsUserInteractive;
            }
        }

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

        public bool IsConsole
        { 
            get
            {
                if (OsInfo.IsWindows)
                {
                    return IsUserInteractive && Process.GetCurrentProcess().ProcessName.Equals(ProcessProvider.NZB_DRONE_CONSOLE_PROCESS_NAME, StringComparison.InvariantCultureIgnoreCase);
                }

                return true;
            } 
        }

        public bool IsRunning { get; set; }
        public bool RestartPending { get; set; }
        public string ExecutingApplication { get; private set; }

        public abstract string RuntimeVersion { get; }

        public static bool IsProduction { get; private set; }

        private static bool InternalIsProduction()
        {
            if (BuildInfo.IsDebug || Debugger.IsAttached) return false;
            if (BuildInfo.Version.Revision > 10000) return false; //Official builds will never have such a high revision

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
				var currentAssmeblyLocation = typeof(RuntimeInfoBase).Assembly.Location;
				if(currentAssmeblyLocation.ToLower().Contains("_output"))return false;
			}
			catch
			{
			
			}

            string lowerCurrentDir = Directory.GetCurrentDirectory().ToLower();
            if (lowerCurrentDir.Contains("teamcity")) return false;
            if (lowerCurrentDir.Contains("_output")) return false;
            if (lowerCurrentDir.StartsWith("/run/")) return false;

            return true;
        }
    }
}