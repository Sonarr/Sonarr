using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NzbDrone.Common
{
    public class EnviromentProvider
    {
        public virtual String LogPath
        {
            get { return Environment.CurrentDirectory; }
        }

        public virtual bool IsUserInteractive
        {
            get { return Environment.UserInteractive; }
        }

        public static bool IsProduction
        {
            get
            {
                if (Debugger.IsAttached) return false;

                var processName = Process.GetCurrentProcess().ProcessName.ToLower();

                Console.WriteLine(processName);
                if (processName.Contains("nunit")) return false;
                if (processName.Contains("jetbrain")) return false;
                if (processName.Contains("resharper")) return false;

                return true;
            }
        }

        public virtual string ApplicationPath
        {
            get
            {
                var dir = new FileInfo(Environment.CurrentDirectory).Directory;

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

        public virtual string StartUpPath
        {
            get
            {
                return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            }
        }

        private static bool ContainsIIS(DirectoryInfo dir)
        {
            return dir.GetDirectories("iisexpress").Length != 0;
        }
    }
}