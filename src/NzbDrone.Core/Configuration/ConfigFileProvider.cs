using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigFileProvider : IHandleAsync<ApplicationStartedEvent>,
                                           IExecute<ResetApiKeyCommand>
    {
        Dictionary<string, object> GetConfigDictionary();
        void SaveConfigDictionary(Dictionary<string, object> configValues);

        string BindAddress { get; }
        int Port { get; }
        int SslPort { get; }
        bool EnableSsl { get; }
        bool LaunchBrowser { get; }
        AuthenticationType AuthenticationMethod { get; }
        bool AnalyticsEnabled { get; }
        string LogLevel { get; }
        string ConsoleLogLevel { get; }
        string Branch { get; }
        string ApiKey { get; }
        string SslCertPath { get; }
        string SslCertPassword { get; }
        string UrlBase { get; }
        string UiFolder { get; }
        bool UpdateAutomatically { get; }
        UpdateMechanism UpdateMechanism { get; }
        string UpdateScriptPath { get; }
    }

    public class ConfigFileProvider : IConfigFileProvider
    {
        public const string CONFIG_ELEMENT_NAME = "Config";

        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskProvider _diskProvider;
        private readonly ICached<string> _cache;

        private readonly string _configFile;
        private static readonly Regex HiddenCharacterRegex = new Regex("[^a-z0-9]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly object Mutex = new object();

        public ConfigFileProvider(IAppFolderInfo appFolderInfo,
                                  ICacheManager cacheManager,
                                  IEventAggregator eventAggregator,
                                  IDiskProvider diskProvider)
        {
            _cache = cacheManager.GetCache<string>(GetType());
            _eventAggregator = eventAggregator;
            _diskProvider = diskProvider;
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
                if (configValue.Key.Equals("ApiKey", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                object currentValue;
                allWithDefaults.TryGetValue(configValue.Key, out currentValue);
                if (currentValue == null)
                {
                    continue;
                }

                var equal = configValue.Value.ToString().Equals(currentValue.ToString());

                if (!equal)
                {
                    SetValue(configValue.Key.FirstCharToUpper(), configValue.Value.ToString());
                }
            }

            _eventAggregator.PublishEvent(new ConfigFileSavedEvent());
        }

        public string BindAddress
        {
            get
            {
                const string defaultValue = "*";

                string bindAddress = GetValue("BindAddress", defaultValue);
                if (string.IsNullOrWhiteSpace(bindAddress))
                {
                    return defaultValue;
                }

                return bindAddress;
            }
        }

        public int Port => GetValueInt("Port", 8989);

        public int SslPort => GetValueInt("SslPort", 9898);

        public bool EnableSsl => GetValueBoolean("EnableSsl", false);

        public bool LaunchBrowser => GetValueBoolean("LaunchBrowser", true);

        public string ApiKey
        {
            get
            {
                var apiKey = GetValue("ApiKey", GenerateApiKey());

                if (apiKey.IsNullOrWhiteSpace())
                {
                    apiKey = GenerateApiKey();
                    SetValue("ApiKey", apiKey);
                }

                return apiKey;
            }
        }

        public AuthenticationType AuthenticationMethod
        {
            get
            {
                var enabled = GetValueBoolean("AuthenticationEnabled", false, false);

                if (enabled)
                {
                    SetValue("AuthenticationMethod", AuthenticationType.Basic);
                    return AuthenticationType.Basic;
                }

                return GetValueEnum("AuthenticationMethod", AuthenticationType.None);
            }
        }

        public bool AnalyticsEnabled => GetValueBoolean("AnalyticsEnabled", true, persist: false);

        public string Branch => GetValue("Branch", "main").ToLowerInvariant();

        public string LogLevel => GetValue("LogLevel", "info").ToLowerInvariant();
        public string ConsoleLogLevel => GetValue("ConsoleLogLevel", string.Empty, persist: false);

        public string SslCertPath => GetValue("SslCertPath", "");
        public string SslCertPassword => GetValue("SslCertPassword", "");

        public string UrlBase
        {
            get
            {
                var urlBase = GetValue("UrlBase", "").Trim('/');

                if (urlBase.IsNullOrWhiteSpace())
                {
                    return urlBase;
                }

                return "/" + urlBase.Trim('/').ToLower();
            }
        }

        public string UiFolder => BuildInfo.IsDebug ? Path.Combine("..", "UI") : "UI";

        public bool UpdateAutomatically => GetValueBoolean("UpdateAutomatically", false, false);

        public UpdateMechanism UpdateMechanism => GetValueEnum("UpdateMechanism", UpdateMechanism.BuiltIn, false);

        public string UpdateScriptPath => GetValue("UpdateScriptPath", "", false);

        public int GetValueInt(string key, int defaultValue)
        {
            return Convert.ToInt32(GetValue(key, defaultValue));
        }

        public bool GetValueBoolean(string key, bool defaultValue, bool persist = true)
        {
            return Convert.ToBoolean(GetValue(key, defaultValue, persist));
        }

        public T GetValueEnum<T>(string key, T defaultValue, bool persist = true)
        {
            return (T)Enum.Parse(typeof(T), GetValue(key, defaultValue), persist);
        }

        public string GetValue(string key, object defaultValue, bool persist = true)
        {
            return _cache.Get(key, () =>
                {
                    var xDoc = LoadConfigFile();
                    var config = xDoc.Descendants(CONFIG_ELEMENT_NAME).Single();

                    var parentContainer = config;

                    var valueHolder = parentContainer.Descendants(key).ToList();

                    if (valueHolder.Count == 1)
                    {
                        return valueHolder.First().Value.Trim();
                    }

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
            var valueString = value.ToString().Trim();
            var xDoc = LoadConfigFile();
            var config = xDoc.Descendants(CONFIG_ELEMENT_NAME).Single();

            var parentContainer = config;

            var keyHolder = parentContainer.Descendants(key);

            if (keyHolder.Count() != 1)
            {
                parentContainer.Add(new XElement(key, valueString));
            }
            else
            {
                parentContainer.Descendants(key).Single().Value = valueString;
            }

            _cache.Set(key, valueString);

            SaveConfigFile(xDoc);
        }

        public void SetValue(string key, Enum value)
        {
            SetValue(key, value.ToString().ToLower());
        }

        private void EnsureDefaultConfigFile()
        {
            if (!File.Exists(_configFile))
            {
                SaveConfigDictionary(GetConfigDictionary());
            }
        }

        private void DeleteOldValues()
        {
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

            SaveConfigFile(xDoc);
        }

        private XDocument LoadConfigFile()
        {
            try
            {
                lock (Mutex)
                {
                    if (_diskProvider.FileExists(_configFile))
                    {
                        var contents = _diskProvider.ReadAllText(_configFile);

                        if (contents.IsNullOrWhiteSpace())
                        {
                            throw new InvalidConfigFileException($"{_configFile} is empty. Please delete the config file and Sonarr will recreate it.");
                        }

                        if (contents.All(char.IsControl))
                        {
                            throw new InvalidConfigFileException($"{_configFile} is corrupt. Please delete the config file and Sonarr will recreate it.");
                        }

                        return XDocument.Parse(_diskProvider.ReadAllText(_configFile));
                    }

                    var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                    xDoc.Add(new XElement(CONFIG_ELEMENT_NAME));

                    return xDoc;
                }
            }
            catch (XmlException ex)
            {
                throw new InvalidConfigFileException($"{_configFile} is corrupt is invalid. Please delete the config file and Sonarr will recreate it.", ex);
            }
        }

        private void SaveConfigFile(XDocument xDoc)
        {
            lock (Mutex)
            {
                _diskProvider.WriteAllText(_configFile, xDoc.ToString());
            }
        }

        private string GenerateApiKey()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            EnsureDefaultConfigFile();
            DeleteOldValues();
        }

        public void Execute(ResetApiKeyCommand message)
        {
            SetValue("ApiKey", GenerateApiKey());
        }
    }
}
