using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using NLog;

namespace NzbDrone.Common
{
    public interface IEnvironmentProvider
    {
        bool IsUserInteractive { get; }
        bool IsAdmin { get; }
        string WorkingDirectory { get; }
        string SystemTemp { get; }
        Version Version { get; }
        DateTime BuildDateTime { get; }
        string StartUpPath { get; }
        Version GetOsVersion();
    }

    public class EnvironmentProvider : IEnvironmentProvider
    {
        private readonly Logger _logger;
        private static readonly string ProcessName = Process.GetCurrentProcess().ProcessName.ToLower();

        private static readonly IEnvironmentProvider Instance = new EnvironmentProvider();


        public EnvironmentProvider()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public static bool IsProduction
        {
            get
            {
                if (IsDebug || Debugger.IsAttached) return false;
                if (Instance.Version.Revision > 10000) return false; //Official builds will never have such a high revision

                var lowerProcessName = ProcessName.ToLower();
                if (lowerProcessName.Contains("vshost")) return false;
                if (lowerProcessName.Contains("nunit")) return false;
                if (lowerProcessName.Contains("jetbrain")) return false;
                if (lowerProcessName.Contains("resharper")) return false;

                return true;
            }
        }

        public static bool IsMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static Guid UGuid { get; set; }

        public virtual bool IsUserInteractive
        {
            get { return Environment.UserInteractive; }
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

        public virtual string WorkingDirectory
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NzbDrone"); }
        }

        public virtual string StartUpPath
        {
            get
            {
                var path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                return path;
            }
        }

        public virtual String SystemTemp
        {
            get
            {
                return Path.GetTempPath();
            }
        }

        public virtual Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public virtual DateTime BuildDateTime
        {
            get
            {
                var fileLocation = Assembly.GetCallingAssembly().Location;
                return new FileInfo(fileLocation).LastWriteTimeUtc;
            }
        }

        public virtual Version GetOsVersion()
        {
            OperatingSystem os = Environment.OSVersion;
            Version version = os.Version;

            return version;
        }
    }
}