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
        }

        public virtual bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser"); }
        }

        public virtual string GetValue(string key, string parent = null)
        {
            var xDoc = XDocument.Load(ConfigFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            if (parent != null)
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
    }
}
