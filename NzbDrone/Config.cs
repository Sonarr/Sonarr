using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using NLog;
using NLog.Config;

namespace NzbDrone
{
    internal class Config
    {
        private static string _projectRoot = string.Empty;

        internal static string ProjectRoot
        {
            get
            {
                if (string.IsNullOrEmpty(_projectRoot))
                {
                    var appDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

                    while (appDir.GetDirectories("iisexpress").Length == 0)
                    {
                        if (appDir.Parent == null) throw new ApplicationException("Can't fine IISExpress folder.");
                        appDir = appDir.Parent;
                    }

                    _projectRoot = appDir.FullName;
                }

                return _projectRoot;
            }
        }

        internal static int Port
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings.Get("port")); }
        }

        internal static void ConfigureNlog()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(
                Path.Combine(ProjectRoot, "NZBDrone.Web\\log.config"), false);
        }
    }
}