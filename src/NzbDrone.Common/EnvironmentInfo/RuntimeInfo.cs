using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.ServiceProcess;
using NLog;

namespace NzbDrone.Common.EnvironmentInfo
{

    public interface IRuntimeInfo
    {
        bool IsUserInteractive { get; }
        bool IsAdmin { get; }
        bool IsWindowsService { get; }
    }

    public class RuntimeInfo : IRuntimeInfo
    {
        private readonly Logger _logger;

        public RuntimeInfo(Logger logger, IServiceProvider serviceProvider)
        {
            _logger = logger;

            IsWindowsService = !IsUserInteractive &&
             OsInfo.IsWindows &&
             serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME) &&
             serviceProvider.GetStatus(ServiceProvider.NZBDRONE_SERVICE_NAME) == ServiceControllerStatus.StartPending;
        }

        public bool IsUserInteractive
        {
            get { return Environment.UserInteractive; }
        }

        static RuntimeInfo()
        {
            IsProduction = InternalIsProduction();


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
                    _logger.WarnException("Error checking if the current user is an administrator.", ex);
                    return false;
                }
            }
        }

        public bool IsWindowsService { get; private set; }

        private static readonly string ProcessName = Process.GetCurrentProcess().ProcessName.ToLower();

        public static bool IsProduction { get; private set; }

        private static bool InternalIsProduction()
        {
            if (BuildInfo.IsDebug || Debugger.IsAttached) return false;
            if (BuildInfo.Version.Revision > 10000) return false; //Official builds will never have such a high revision

            var lowerProcessName = ProcessName.ToLower();
            if (lowerProcessName.Contains("vshost")) return false;
            if (lowerProcessName.Contains("nunit")) return false;
            if (lowerProcessName.Contains("jetbrain")) return false;
            if (lowerProcessName.Contains("resharper")) return false;

            string lowerCurrentDir = Directory.GetCurrentDirectory().ToLower();
            if (lowerCurrentDir.Contains("teamcity")) return false;
            if (lowerCurrentDir.StartsWith("/run/")) return false;

            return true;
        }
    }
}