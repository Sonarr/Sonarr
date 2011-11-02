using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Model;

namespace NzbDrone.Providers
{
    public class ConfigProvider
    {
        private readonly EnviromentProvider _enviromentProvider;
        private static readonly Logger Logger = LogManager.GetLogger("Host.ConfigProvider");

        [Inject]
        public ConfigProvider(EnviromentProvider enviromentProvider)
        {
            _enviromentProvider = enviromentProvider;
        }

        public ConfigProvider()
        {

        }

        public virtual int PortNumber
        {
            get { return GetValueInt("Port", 8989); }
        }

        public virtual bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser", true); }
        }

        public virtual string IISDirectory
        {
            get { return Path.Combine(_enviromentProvider.ApplicationPath, "IISExpress"); }
        }

        public virtual string IISExePath
        {
            get { return Path.Combine(IISDirectory, "iisexpress.exe"); }
        }

        public virtual string IISConfigPath
        {
            get { return Path.Combine(IISDirectory, "AppServer", "applicationhost.config"); }
        }

        public virtual string AppDataDirectory
        {
            get { return Path.Combine(_enviromentProvider.ApplicationPath, "NzbDrone.Web", "App_Data"); }
        }

        public virtual string ConfigFile
        {
            get { return Path.Combine(AppDataDirectory, "Config.xml"); }
        }

        public virtual string NlogConfigPath
        {
            get { return Path.Combine(_enviromentProvider.ApplicationPath, "NzbDrone.Web\\log.config"); }
        }

        public virtual AuthenticationType AuthenticationType
        {
            get { return (AuthenticationType)GetValueInt("AuthenticationType", 0); }
        }

        public virtual void UpdateIISConfig(string configPath)
        {
            Logger.Info(@"Server configuration file: {0}", configPath);
            Logger.Info(@"Configuring server to: [http://localhost:{0}]", PortNumber);

            var configXml = XDocument.Load(configPath);

            var bindings =
                configXml.XPathSelectElement("configuration/system.applicationHost/sites").Elements("site").Where(
                    d => d.Attribute("name").Value.ToLowerInvariant() == "nzbdrone").First().Element("bindings");
            bindings.Descendants().Remove();
            bindings.Add(
                new XElement("binding",
                             new XAttribute("protocol", "http"),
                             new XAttribute("bindingInformation", String.Format("*:{0}:localhost", PortNumber))
                    ));

            bindings.Add(
                new XElement("binding",
                             new XAttribute("protocol", "http"),
                             new XAttribute("bindingInformation", String.Format("*:{0}:", PortNumber))
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

        private string GetValue(string key, object defaultValue, string parent = null)
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