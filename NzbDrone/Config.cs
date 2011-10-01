using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
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
            get { return GetValueInt("Port"); }
        }

        internal static bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser"); }
        }

        internal static string AppDataDirectory
        {
            get { return Path.Combine(ProjectRoot, "NzbDrone.Web", "App_Data"); }
        }

        internal static string ConfigFile
        {
            get { return Path.Combine(AppDataDirectory, "Config.xml"); }
        }

        internal static void ConfigureNlog()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(
                Path.Combine(ProjectRoot, "NzbDrone.Web\\log.config"), false);
        }

        internal static void CreateDefaultConfigFile()
        {
            //Create the config file here
            Directory.CreateDirectory(AppDataDirectory);

            if (!File.Exists(ConfigFile))
            {
                WriteDefaultConfig();
            }
        }

        internal static void WriteDefaultConfig()
        {
            var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

            xDoc.Add(new XElement("Config",
                                    new XElement("Port", 8989),
                                    new XElement("LaunchBrowser", true)
                                 )
                    );

            xDoc.Save(ConfigFile);
        }

        private static string GetValue(string key, string parent = null)
        {
            var xDoc = XDocument.Load(ConfigFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            if (parent != null)
                parentContainer = config.Descendants(parent).Single();

            var value = parentContainer.Descendants(key).Single().Value;

            return value;
        }

        private static int GetValueInt(string key, string parent = null)
        {
            return Convert.ToInt32(GetValue(key, parent));
        }

        private static bool GetValueBoolean(string key, string parent = null)
        {
            return Convert.ToBoolean(GetValue(key, parent));
        }
    }
}