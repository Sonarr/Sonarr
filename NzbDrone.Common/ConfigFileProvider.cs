using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NLog;
using Ninject;
using NzbDrone.Common.Model;

namespace NzbDrone.Common
{
    public class ConfigFileProvider
    {
        private readonly EnvironmentProvider _environmentProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly string _configFile;
        
        [Inject]
        public ConfigFileProvider(EnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
            _configFile = _environmentProvider.GetConfigPath();

            CreateDefaultConfigFile();
        }

        public ConfigFileProvider()
        {
            
        }

        public virtual Guid Guid
        {
            get
            {
                var key = "Guid";
                if (string.IsNullOrWhiteSpace(GetValue(key, string.Empty)))
                {
                    SetValue(key, Guid.NewGuid().ToString());
                }

                return Guid.Parse(GetValue(key, string.Empty));
            }
        }

        public virtual int Port
        {
            get { return GetValueInt("Port", 8989); }
            set { SetValue("Port", value); }
        }

        public virtual bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser", true); }
            set { SetValue("LaunchBrowser", value); }
        }

        public virtual AuthenticationType AuthenticationType
        {
            get { return (AuthenticationType)GetValueInt("AuthenticationType", 0); }
            set { SetValue("AuthenticationType", (int)value); }
        }

        public virtual bool EnableProfiler
        {
            get { return GetValueBoolean("EnableProfiler", false); }
            set { SetValue("EnableProfiler", value); }
        }

        public virtual int GetValueInt(string key, int defaultValue)
        {
            return Convert.ToInt32(GetValue(key, defaultValue));
        }

        public virtual bool GetValueBoolean(string key, bool defaultValue)
        {
            return Convert.ToBoolean(GetValue(key, defaultValue));
        }

        public virtual string GetValue(string key, object defaultValue)
        {
            var xDoc = XDocument.Load(_configFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;


            var valueHolder = parentContainer.Descendants(key).ToList();

            if (valueHolder.Count() == 1)
                return valueHolder.First().Value;

            //Save the value
            SetValue(key, defaultValue);

            //return the default value
            return defaultValue.ToString();
        }


        public virtual void SetValue(string key, object value)
        {
            var xDoc = XDocument.Load(_configFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            var keyHolder = parentContainer.Descendants(key);

            if (keyHolder.Count() != 1)
                parentContainer.Add(new XElement(key, value));

            else
                parentContainer.Descendants(key).Single().Value = value.ToString();

            xDoc.Save(_configFile);
        }

        private void CreateDefaultConfigFile()
        {
            if (!File.Exists(_configFile))
            {
                var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

                xDoc.Add(new XElement("Config"));

                xDoc.Save(_configFile);
            }
        }

        public virtual void UpdateIISConfig(string configPath)
        {
            logger.Info(@"Server configuration file: {0}", configPath);
            logger.Info(@"Configuring server to: [http://localhost:{0}]", Port);

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
    }
}
