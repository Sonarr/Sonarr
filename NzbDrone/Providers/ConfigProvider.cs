using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using NLog;
using NLog.Config;
using NzbDrone.Model;

namespace NzbDrone.Providers
{
    public class ConfigProvider
    {
        private static readonly Logger Logger = LogManager.GetLogger("Host.ConfigProvider");

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
            get { return GetValueInt("Port", 8989); }
        }

        public virtual bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser", true); }
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

        public virtual AuthenticationType AuthenticationType
        {
            get { return (AuthenticationType)GetValueInt("AuthenticationType", 0); }
            set { SetValue("AuthenticationType", (int)value); }
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

            //Update the authenticationTypes

            var location = configXml.XPathSelectElement("configuration").Elements("location").Where(
                    d => d.Attribute("path").Value.ToLowerInvariant() == "nzbdrone").First();


            var authenticationTypes = location.XPathSelectElements("system.webServer/security/authentication").First().Descendants();

            //Set all authentication types enabled to false
            foreach (var child in authenticationTypes)
            {
                child.Attribute("enabled").Value = "false";
            }

            var configuredAuthType = String.Format("{0}Authentication", AuthenticationType.ToString()).ToLowerInvariant();

            //Set the users authenticationType to true
            authenticationTypes.Where(t => t.Name.ToString().ToLowerInvariant() == configuredAuthType).Single().Attribute("enabled").Value = "true";

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

        public virtual string GetValue(string key, object defaultValue, string parent = null)
        {
            var xDoc = XDocument.Load(ConfigFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            if (!String.IsNullOrEmpty(parent))
            {
                //Add the parent
                if (config.Descendants(parent).Count() != 1)
                {
                    SetValue(key, defaultValue, parent);

                    //Reload the configFile
                    xDoc = XDocument.Load(ConfigFile);
                    config = xDoc.Descendants("Config").Single();
                }

                parentContainer = config.Descendants(parent).Single();
            }

            var valueHolder = parentContainer.Descendants(key).ToList();

            if (valueHolder.Count() == 1)
                return valueHolder.First().Value;

            //Save the value
            SetValue(key, defaultValue, parent);

            //return the default value
            return defaultValue.ToString();
        }

        public virtual int GetValueInt(string key, int defaultValue, string parent = null)
        {
            return Convert.ToInt32(GetValue(key, defaultValue, parent));
        }

        public virtual bool GetValueBoolean(string key, bool defaultValue, string parent = null)
        {
            return Convert.ToBoolean(GetValue(key, defaultValue, parent));
        }

        public virtual void SetValue(string key, object value, string parent = null)
        {
            var xDoc = XDocument.Load(ConfigFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            if (!String.IsNullOrEmpty(parent))
            {
                //Add the parent container if it doesn't already exist
                if (config.Descendants(parent).Count() != 1)
                {
                    config.Add(new XElement(parent));
                }

                parentContainer = config.Descendants(parent).Single();
            }

            var keyHolder = parentContainer.Descendants(key);

            if (keyHolder.Count() != 1)
                parentContainer.Add(new XElement(key, value));

            else
                parentContainer.Descendants(key).Single().Value = value.ToString();

            xDoc.Save(ConfigFile);
        }

        public virtual void WriteDefaultConfig()
        {
            var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

            xDoc.Add(new XElement("Config"));

            xDoc.Save(ConfigFile);
        }
    }
}