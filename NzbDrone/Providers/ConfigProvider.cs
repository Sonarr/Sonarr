using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using NLog;
using NLog.Config;

namespace NzbDrone.Providers
{
    public class ConfigProvider
    {
        private static readonly Logger Logger = LogManager.GetLogger("ConfigProvider");

        public virtual string ApplicationRoot
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

        public virtual int Port
        {
            get { return GetValueInt("Port"); }
        }

        public virtual bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser"); }
        }

        public virtual string AppDataDirectory
        {
            get { return Path.Combine(ApplicationRoot, "NzbDrone.Web", "App_Data"); }
        }

        public virtual string ConfigFile
        {
            get { return Path.Combine(AppDataDirectory, "Config.xml"); }
        }

        public virtual string IISFolder
        {
            get { return Path.Combine(ApplicationRoot, @"IISExpress\"); }
        }

        public virtual string IISExePath
        {
            get { return IISFolder + @"iisexpress.exe"; }
        }

        public virtual string IISConfigPath
        {
            get { return Path.Combine(IISFolder, "AppServer", "applicationhost.config"); }
        }

        public virtual void ConfigureNlog()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(
                Path.Combine(ApplicationRoot, "NzbDrone.Web\\log.config"), false);
        }

        public virtual void UpdateIISConfig(string configPath)
        {
            Logger.Info(@"Server configuration file: {0}", configPath);
            Logger.Info(@"Configuring server to: [http://localhost:{0}]", Port);

            var configXml = XDocument.Load(configPath);

            var bindings =
                configXml.XPathSelectElement("configuration/system.applicationHost/sites").Elements("site").Where(
                    d => d.Attribute("name").Value.ToLowerInvariant() == "nzbdrone").First().Element("bindings");
            bindings.Descendants().Remove();
            bindings.Add(
                new XElement("binding",
                             new XAttribute("protocol", "http"),
                             new XAttribute("bindingInformation", String.Format("*:{0}:localhost", Port))
                    ));

            bindings.Add(
            new XElement("binding",
                         new XAttribute("protocol", "http"),
                         new XAttribute("bindingInformation", String.Format("*:{0}:", Port))
                ));

            configXml.Save(configPath);
        }

        public virtual void CreateDefaultConfigFile()
        {
            //Create the config file here
            Directory.CreateDirectory(AppDataDirectory);

            if (!File.Exists(ConfigFile))
            {
                WriteDefaultConfig();
            }
        }

        public virtual void WriteDefaultConfig()
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