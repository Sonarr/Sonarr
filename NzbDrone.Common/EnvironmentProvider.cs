using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NzbDrone.Common
{
    public class EnvironmentProvider
    {
        private static readonly string ProcessName = Process.GetCurrentProcess().ProcessName.ToLower();

        private static readonly EnvironmentProvider Instance = new EnvironmentProvider();

        private const string NZBDRONE_PID = "NZBDRONE_PID";

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

                if (Instance.StartUpPath.ToLower().Contains("_rawpackage")) return false;

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

        public virtual string WorkingDirectory
        {
            get { return Directory.GetCurrentDirectory(); }
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
                return new FileInfo(fileLocation).CreationTime;
            }
        }

        public virtual int NzbDroneProcessIdFromEnvironment
        {
            get
            {
                var id = Convert.ToInt32(Environment.GetEnvironmentVariable(NZBDRONE_PID));

                if (id == 0)
                    throw new InvalidOperationException("NZBDRONE_PID isn't a valid environment variable.");

                return id;
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