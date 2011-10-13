using System;
using System.IO;
using System.Reflection;

namespace NzbDrone.Providers
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

        public virtual string ApplicationPath
        {
            get
            {
                var dir = new FileInfo(Environment.CurrentDirectory).Directory;

                while (dir.GetDirectories("iisexpress").Length == 0)
                {
                    if (dir.Parent == null) break;
                    dir = dir.Parent;
                }

                dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
                
                while (dir.GetDirectories("iisexpress").Length == 0)
                {
                    if (dir.Parent == null) throw new ApplicationException("Can't fine IISExpress folder.");
                    dir = dir.Parent;
                }
                
                return dir.FullName;
            }
        }
    }
}