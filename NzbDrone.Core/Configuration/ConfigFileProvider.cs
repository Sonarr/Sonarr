using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Model;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigFileProvider
    {
        Dictionary<string, object> GetConfigDictionary();
        void SaveConfigDictionary(Dictionary<string, object> configValues);

        int Port { get; set; }
        bool LaunchBrowser { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string BasicAuthUsername { get; set; }
        string BasicAuthPassword { get; set; }
    }

    public class ConfigFileProvider : IConfigFileProvider
    {
        private readonly IAppDirectoryInfo _appDirectoryInfo;
        private readonly ICached<string> _cache;

        private readonly string _configFile;

        public ConfigFileProvider(IAppDirectoryInfo appDirectoryInfo, ICacheManger cacheManger)
        {
            _appDirectoryInfo = appDirectoryInfo;
            _cache = cacheManger.GetCache<string>(this.GetType());
            _configFile = _appDirectoryInfo.GetConfigPath();
        }

        public Dictionary<string, object> GetConfigDictionary()
        {
            var dict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            var type = GetType();
            var properties = type.GetProperties();

            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(this, null);

                dict.Add(propertyInfo.Name, value);
            }

            return dict;
        }

        public void SaveConfigDictionary(Dictionary<string, object> configValues)
        {
            _cache.Clear();

            var allWithDefaults = GetConfigDictionary();

            foreach (var configValue in configValues)
            {
                object currentValue;
                allWithDefaults.TryGetValue(configValue.Key, out currentValue);
                if (currentValue == null) continue;

                var equal = configValue.Value.ToString().Equals(currentValue.ToString());

                if (!equal)
                {
                    SetValue(configValue.Key.FirstCharToUpper(), configValue.Value.ToString());
                }
            }


        }

        public int Port
        {
            get { return GetValueInt("Port", 8989); }
            set { SetValue("Port", value); }
        }

        public bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser", true); }
            set { SetValue("LaunchBrowser", value); }
        }

        public AuthenticationType AuthenticationType
        {
            get { return GetValueEnum("AuthenticationType", AuthenticationType.Anonymous); }
            set { SetValue("AuthenticationType", value); }
        }

        public string BasicAuthUsername
        {
            get { return GetValue("BasicAuthUsername", ""); }
            set { SetValue("BasicAuthUsername", value); }
        }

        public string BasicAuthPassword
        {
            get { return GetValue("BasicAuthPassword", ""); }
            set { SetValue("BasicAuthPassword", value); }
        }

        public int GetValueInt(string key, int defaultValue)
        {
            return Convert.ToInt32(GetValue(key, defaultValue));
        }

        public bool GetValueBoolean(string key, bool defaultValue)
        {
            return Convert.ToBoolean(GetValue(key, defaultValue));
        }

        public T GetValueEnum<T>(string key, T defaultValue)
        {
            return (T)Enum.Parse(typeof(T), GetValue(key, defaultValue), true);
        }

        public string GetValue(string key, object defaultValue)
        {
            return _cache.Get(key, () =>
                  {
                      EnsureDefaultConfigFile();

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
                  });
        }

        public void SetValue(string key, object value)
        {
            EnsureDefaultConfigFile();

            var xDoc = XDocument.Load(_configFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            var keyHolder = parentContainer.Descendants(key);

            if (keyHolder.Count() != 1)
            {
                parentContainer.Add(new XElement(key, value));
            }

            else
            {
                parentContainer.Descendants(key).Single().Value = value.ToString();
            }

            _cache.Set(key, value.ToString());

            xDoc.Save(_configFile);
        }

        public void SetValue(string key, Enum value)
        {
            SetValue(key, value.ToString().ToLower());
        }

        private void EnsureDefaultConfigFile()
        {
            if (!File.Exists(_configFile))
            {
                var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                xDoc.Add(new XElement("Config"));
                xDoc.Save(_configFile);
            }
        }
    }
}
