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
    }

    public class RuntimeInfo : IRuntimeInfo
    {
        private readonly Logger _logger;

        public RuntimeInfo(Logger logger)
        {
            _logger = logger;
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

            if (Directory.GetCurrentDirectory().ToLower().Contains("teamcity")) return false;

            return true;
        }
    }
}