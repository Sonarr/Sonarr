using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NLog;
using NLog.Config;

namespace NzbDrone.Providers
{
    internal class ConfigProvider
    {

        internal virtual string ApplicationRoot
        {
            get
            {
                var appDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

                while (appDir.GetDirectories("iisexpress").Length == 0)
                {
                    if (appDir.Parent == null) throw new ApplicationException("Can't fine IISExpress folder.");
                    appDir = appDir.Parent;
                }

                return appDir.FullName;
            }
        }

        internal virtual int Port
        {
            get { return GetValueInt("Port"); }
        }

        internal virtual bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser"); }
        }

        internal virtual string AppDataDirectory
        {
            get { return Path.Combine(ApplicationRoot, "NzbDrone.Web", "App_Data"); }
        }

        internal virtual string ConfigFile
        {
            get { return Path.Combine(AppDataDirectory, "Config.xml"); }
        }

        internal virtual string IISFolder
        {
            get { return Path.Combine(ApplicationRoot, @"IISExpress\"); }
        }

        internal virtual void ConfigureNlog()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(
                Path.Combine(ApplicationRoot, "NzbDrone.Web\\log.config"), false);
        }

        internal virtual void CreateDefaultConfigFile()
        {
            //Create the config file here
            Directory.CreateDirectory(AppDataDirectory);

            if (!File.Exists(ConfigFile))
            {
                WriteDefaultConfig();
            }
        }

        internal virtual void WriteDefaultConfig()
        {
            var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

            xDoc.Add(new XElement("Config",
                                    new XElement("Port", 8989),
                                    new XElement("LaunchBrowser", true)
                                 )
                    );

            xDoc.Save(ConfigFile);
        }

        private string GetValue(string key, string parent = null)
        {
            var xDoc = XDocument.Load(ConfigFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            if (parent != null)
                parentContainer = config.Descendants(parent).Single();

            var value = parentContainer.Descendants(key).Single().Value;

            return value;
        }

        private int GetValueInt(string key, string parent = null)
        {
            return Convert.ToInt32(GetValue(key, parent));
        }

        private bool GetValueBoolean(string key, string parent = null)
        {
            return Convert.ToBoolean(GetValue(key, parent));
        }
    }
}