using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.Core
{
    public class ConfigFileProvider
    {
        private string _configFile = Path.Combine(CentralDispatch.AppPath, "App_Data", "Config.xml");

        public string ConfigFile
        {
            get { return _configFile; }
            set { _configFile = value; }
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

        public virtual void CreateDefaultConfigFile()
        {
            //Create the config file here
            Directory.CreateDirectory(Path.Combine(CentralDispatch.AppPath, "App_Data"));

            if (!File.Exists(ConfigFile))
            {
                WriteDefaultConfig();
            }
        }

        public virtual void WriteDefaultConfig()
        {
            var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

            xDoc.Add(new XElement("Config"));

            xDoc.Save(ConfigFile);
        }
    }
}
