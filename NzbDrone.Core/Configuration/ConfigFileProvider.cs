using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Configuration
{
    public interface IConfigFileProvider : IHandleAsync<ApplicationStartedEvent>
    {
        Dictionary<string, object> GetConfigDictionary();
        void SaveConfigDictionary(Dictionary<string, object> configValues);

        int Port { get; }
        bool LaunchBrowser { get; }
        bool AuthenticationEnabled { get; }
        string Username { get; }
        string Password { get; }
        string LogLevel { get; }
        string Branch { get; }
        bool Torrent { get; }
    }

    public class ConfigFileProvider : IConfigFileProvider
    {
        private const string CONFIG_ELEMENT_NAME = "Config";

        private readonly IEventAggregator _eventAggregator;
        private readonly ICached<string> _cache;

        private readonly string _configFile;

        public ConfigFileProvider(IAppFolderInfo appFolderInfo, ICacheManger cacheManger, IEventAggregator eventAggregator)
        {
            _cache = cacheManger.GetCache<string>(GetType());
            _eventAggregator = eventAggregator;
            _configFile = appFolderInfo.GetConfigPath();
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

            _eventAggregator.PublishEvent(new ConfigFileSavedEvent());
        }

        public int Port
        {
            get { return GetValueInt("Port", 8989); }
        }

        public bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser", true); }
        }

        public bool Torrent
        {
            get { return GetValueBoolean("Torrent", false, persist: false); }
        }

        public bool AuthenticationEnabled
        {
            get { return GetValueBoolean("AuthenticationEnabled", false); }
        }

        public string Branch
        {
            get { return GetValue("Branch", "master"); }
        }

        public string Username
        {
            get { return GetValue("Username", ""); }
        }

        public string Password
        {
            get { return GetValue("Password", ""); }
        }

        public string LogLevel
        {
            get { return GetValue("LogLevel", "Info"); }
        }

        public int GetValueInt(string key, int defaultValue)
        {
            return Convert.ToInt32(GetValue(key, defaultValue));
        }

        public bool GetValueBoolean(string key, bool defaultValue, bool persist = true)
        {
            return Convert.ToBoolean(GetValue(key, defaultValue, persist));
        }

        public T GetValueEnum<T>(string key, T defaultValue)
        {
            return (T)Enum.Parse(typeof(T), GetValue(key, defaultValue), true);
        }

        public string GetValue(string key, object defaultValue, bool persist = true)
        {
            return _cache.Get(key, () =>
                {
                    EnsureDefaultConfigFile();

                    var xDoc = LoadConfigFile();
                    var config = xDoc.Descendants(CONFIG_ELEMENT_NAME).Single();

                    var parentContainer = config;

                    var valueHolder = parentContainer.Descendants(key).ToList();

                    if (valueHolder.Count() == 1)
                        return valueHolder.First().Value;

                    //Save the value
                    if (persist)
                    {
                        SetValue(key, defaultValue);
                    }

                    //return the default value
                    return defaultValue.ToString();
                });
        }

        public void SetValue(string key, object value)
        {
            EnsureDefaultConfigFile();

            var xDoc = LoadConfigFile();
            var config = xDoc.Descendants(CONFIG_ELEMENT_NAME).Single();

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
                xDoc.Add(new XElement(CONFIG_ELEMENT_NAME));
                xDoc.Save(_configFile);
            }
        }

        private void DeleteOldValues()
        {
            EnsureDefaultConfigFile();

            var xDoc = LoadConfigFile();
            var config = xDoc.Descendants(CONFIG_ELEMENT_NAME).Single();

            var type = GetType();
            var properties = type.GetProperties();

            foreach (var configValue in config.Descendants().ToList())
            {
                var name = configValue.Name.LocalName;

                if (!properties.Any(p => p.Name == name))
                {
                    config.Descendants(name).Remove();
                }
            }

            xDoc.Save(_configFile);
        }

        private XDocument LoadConfigFile()
        {
            try
            {
                return XDocument.Load(_configFile);
            }

            catch (XmlException ex)
            {
                throw new InvalidConfigFileException(_configFile + " is invalid, please see the http://wiki.nzbdrone.com for steps to resolve this issue.", ex);
            }
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            DeleteOldValues();
        }
    }
}
