using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace NzbDrone.Core.Providers.Core
{
    public class ConfigFileProvider
    {
        public string ConfigFile
        {
            get { return Path.Combine(CentralDispatch.AppPath, "App_Data", "Config.xml"); }
        }
        
        public virtual int Port
        {
            get { return GetValueInt("Port"); }
            set { SetValue("Port", value); }
        }

        public virtual bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser"); }
            set { SetValue("LaunchBrowser", value); }
        }

        public virtual string GetValue(string key, string parent = null)
        {
            var xDoc = XDocument.Load(ConfigFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            if (!String.IsNullOrEmpty(parent))
                parentContainer = config.Descendants(parent).Single();

            var value = parentContainer.Descendants(key).Single().Value;

            return value;
        }

        public virtual int GetValueInt(string key, string parent = null)
        {
            return Convert.ToInt32(GetValue(key, parent));
        }

        public virtual bool GetValueBoolean(string key, string parent = null)
        {
            return Convert.ToBoolean(GetValue(key, parent));
        }

        public virtual void SetValue(string key, object value, string parent = null)
        {
            var xDoc = XDocument.Load(ConfigFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            if (!String.IsNullOrEmpty(parent))
                parentContainer = config.Descendants(parent).Single();

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

            xDoc.Add(new XElement("Config",
                                    new XElement("Port", 8989),
                                    new XElement("LaunchBrowser", true)
                                 )
                    );

            xDoc.Save(ConfigFile);
        }
    }
}
