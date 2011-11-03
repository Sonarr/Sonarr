using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NzbDrone.Common
{
    public class EnviromentProvider
    {
        public const string IIS_FOLDER_NAME = "iisexpress";

#if DEBUG
        private static readonly bool isInDebug = true;
#else
        private static readonly bool isInDebug = false; 
#endif

        private static readonly string processName = Process.GetCurrentProcess().ProcessName.ToLower();

        public static bool IsProduction
        {
            get
            {
                if (isInDebug || Debugger.IsAttached) return false;

                Console.WriteLine(processName);
                if (processName.Contains("nunit")) return false;
                if (processName.Contains("jetbrain")) return false;
                if (processName.Contains("resharper")) return false;

                return true;
            }
        }

        public virtual String LogPath
        {
            get { return Environment.CurrentDirectory; }
        }

        public virtual bool IsUserInteractive
        {
            get { return Environment.UserInteractive; }
        }

        public virtual string ApplicationPath
        {
            get
            {
                var dir = new DirectoryInfo(Environment.CurrentDirectory);

                while (!ContainsIIS(dir))
                {
                    if (dir.Parent == null) break;
                    dir = dir.Parent;
                }

                if (ContainsIIS(dir)) return dir.FullName;

                dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

                while (!ContainsIIS(dir))
                {
                    if (dir.Parent == null) throw new ApplicationException("Can't fine IISExpress folder.");
                    dir = dir.Parent;
                }

                return dir.FullName;
            }
        }

        public virtual string WebRoot
        {
            get
            {
                return Path.Combine(ApplicationPath, "NzbDrone.Web");
            }
        }

        public virtual string AppDataPath
        {
            get
            {
                var path = Path.Combine(WebRoot, "App_Data");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public virtual string StartUpPath
        {
            get
            {
                return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
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


        public virtual String TempPath
        {
            get
            {
                return Path.GetTempPath();
            }
        }

        private static bool ContainsIIS(DirectoryInfo dir)
        {
            return dir.GetDirectories(IIS_FOLDER_NAME).Length != 0;
        }
    }
}