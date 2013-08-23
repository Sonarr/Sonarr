using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Download.Clients.Sabnzbd;

namespace NzbDrone.Core.Configuration
{
    public enum ConfigKey
    {
        DownloadedEpisodesFolder
    }

    public class ConfigService : IConfigService
    {
        private readonly IConfigRepository _repository;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;
        private static Dictionary<string, string> _cache;

        public ConfigService(IConfigRepository repository, IMessageAggregator messageAggregator, Logger logger)
        {
            _repository = repository;
            _messageAggregator = messageAggregator;
            _logger = logger;
            _cache = new Dictionary<string, string>();
        }

        public IEnumerable<Config> All()
        {
            return _repository.All();
        }

        public Dictionary<String, Object> AllWithDefaults()
        {
            var dict = new Dictionary<String, Object>(StringComparer.InvariantCultureIgnoreCase);

            var type = GetType();
            var properties = type.GetProperties();

            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(this, null);

                dict.Add(propertyInfo.Name, value);
            }

            return dict;
        }

        public void SaveValues(Dictionary<string, object> configValues)
        {
            var allWithDefaults = AllWithDefaults();

            foreach (var configValue in configValues)
            {
                object currentValue;
                allWithDefaults.TryGetValue(configValue.Key, out currentValue);
                if (currentValue == null) continue;

                var equal = configValue.Value.ToString().Equals(currentValue.ToString());

                if (!equal)
                    SetValue(configValue.Key, configValue.Value.ToString());
            }

            _messageAggregator.PublishEvent(new ConfigSavedEvent());
        }

        public String SabHost
        {
            get { return GetValue("SabHost", "localhost"); }

            set { SetValue("SabHost", value); }
        }

        public int SabPort
        {
            get { return GetValueInt("SabPort", 8080); }

            set { SetValue("SabPort", value); }
        }

        public String SabApiKey
        {
            get { return GetValue("SabApiKey"); }

            set { SetValue("SabApiKey", value); }
        }

        public String SabUsername
        {
            get { return GetValue("SabUsername"); }

            set { SetValue("SabUsername", value); }
        }

        public String SabPassword
        {
            get { return GetValue("SabPassword"); }

            set { SetValue("SabPassword", value); }
        }

        public String SabTvCategory
        {
            get { return GetValue("SabTvCategory", "tv"); }

            set { SetValue("SabTvCategory", value); }
        }

        public SabPriorityType SabRecentTvPriority
        {
            get { return GetValueEnum("SabRecentTvPriority", SabPriorityType.Default); }

            set { SetValue("SabRecentTvPriority", value); }
        }

        public SabPriorityType SabOlderTvPriority
        {
            get { return GetValueEnum("SabOlderTvPriority", SabPriorityType.Default); }

            set { SetValue("SabOlderTvPriority", value); }
        }

        public String DownloadedEpisodesFolder
        {
            get { return GetValue(ConfigKey.DownloadedEpisodesFolder.ToString()); }

            set { SetValue(ConfigKey.DownloadedEpisodesFolder.ToString(), value); }
        }

        public bool UseSeasonFolder
        {
            get { return GetValueBoolean("UseSeasonFolder", true); }

            set { SetValue("UseSeasonFolder", value); }
        }

        public string SeasonFolderFormat
        {
            get { return GetValue("SeasonFolderFormat", "Season %s"); }
            set { SetValue("SeasonFolderFormat", value); }
        }

        public bool AutoUnmonitorPreviouslyDownloadedEpisodes
        {
            get { return GetValueBoolean("AutoUnmonitorPreviouslyDownloadedEpisodes"); }
            set { SetValue("AutoUnmonitorPreviouslyDownloadedEpisodes", value); }
        }

        public int Retention
        {
            get { return GetValueInt("Retention", 0); }
            set { SetValue("Retention", value); }
        }

        public DownloadClientType DownloadClient
        {
            get { return GetValueEnum("DownloadClient", DownloadClientType.Sabnzbd); }

            set { SetValue("DownloadClient", value); }
        }

        public string BlackholeFolder
        {
            get { return GetValue("BlackholeFolder", String.Empty); }
            set { SetValue("BlackholeFolder", value); }
        }

        public string ServiceRootUrl
        {
            get { return "http://services.nzbdrone.com"; }
        }

        public string PneumaticFolder
        {
            get { return GetValue("PneumaticFolder", String.Empty); }
            set { SetValue("PneumaticFolder", value); }
        }

        public string RecycleBin
        {
            get { return GetValue("RecycleBin", String.Empty); }
            set { SetValue("RecycleBin", value); }
        }

        public String NzbgetUsername
        {
            get { return GetValue("NzbgetUsername", "nzbget"); }

            set { SetValue("NzbgetUsername", value); }
        }

        public String NzbgetPassword
        {
            get { return GetValue("NzbgetPassword", ""); }

            set { SetValue("NzbgetPassword", value); }
        }

        public String NzbgetHost
        {
            get { return GetValue("NzbgetHost", "localhost"); }

            set { SetValue("NzbgetHost", value); }
        }

        public Int32 NzbgetPort
        {
            get { return GetValueInt("NzbgetPort", 6789); }

            set { SetValue("NzbgetPort", value); }
        }

        public String NzbgetTvCategory
        {
            get { return GetValue("NzbgetTvCategory", "nzbget"); }

            set { SetValue("NzbgetTvCategory", value); }
        }

        public PriorityType NzbgetRecentTvPriority
        {
            get { return GetValueEnum("NzbgetRecentTvPriority", PriorityType.Normal); }

            set { SetValue("NzbgetRecentTvPriority", value); }
        }

        public PriorityType NzbgetOlderTvPriority
        {
            get { return GetValueEnum("NzbgetOlderTvPriority", PriorityType.Normal); }

            set { SetValue("NzbgetOlderTvPriority", value); }
        }

        public string ReleaseRestrictions
        {
            get { return GetValue("ReleaseRestrictions", String.Empty); }
            set { SetValue("ReleaseRestrictions", value); }
        }

        public Int32 RssSyncInterval
        {
            get { return GetValueInt("RssSyncInterval", 15); }

            set { SetValue("RssSyncInterval", value); }
        }

        public Boolean AutoDownloadPropers
        {
            get { return GetValueBoolean("AutoDownloadPropers", true); }

            set { SetValue("AutoDownloadPropers", value); }
        }

        public string DownloadClientWorkingFolders
        {
            get { return GetValue("DownloadClientWorkingFolders", "_UNPACK_|_FAILED_"); }
            set { SetValue("DownloadClientWorkingFolders", value); }
        }

        private string GetValue(string key)
        {
            return GetValue(key, String.Empty);
        }

        private bool GetValueBoolean(string key, bool defaultValue = false)
        {
            return Convert.ToBoolean(GetValue(key, defaultValue));
        }

        private int GetValueInt(string key, int defaultValue = 0)
        {
            return Convert.ToInt32(GetValue(key, defaultValue));
        }

        public T GetValueEnum<T>(string key, T defaultValue)
        {
            return (T)Enum.Parse(typeof(T), GetValue(key, defaultValue), true);
        }

        public string GetValue(string key, object defaultValue, bool persist = false)
        {
            EnsureCache();

            key = key.ToLowerInvariant();
            string dbValue;

            if (_cache.TryGetValue(key, out dbValue) && dbValue != null && !String.IsNullOrEmpty(dbValue))
                return dbValue;

            _logger.Trace("Unable to find config key '{0}' defaultValue:'{1}'", key, defaultValue);

            if (persist)
            {
                SetValue(key, defaultValue.ToString());
            }
            return defaultValue.ToString();
        }

        private void SetValue(string key, Boolean value)
        {
            SetValue(key, value.ToString());
        }

        private void SetValue(string key, int value)
        {
            SetValue(key, value.ToString());
        }

        public void SetValue(string key, string value)
        {
            key = key.ToLowerInvariant();

            if (String.IsNullOrEmpty(key))
                throw new ArgumentOutOfRangeException("key");
            if (value == null)
                throw new ArgumentNullException("key");

            _logger.Trace("Writing Setting to file. Key:'{0}' Value:'{1}'", key, value);

            var dbValue = _repository.Get(key);

            if (dbValue == null)
            {
                _repository.Insert(new Config { Key = key, Value = value });
            }
            else
            {
                dbValue.Value = value;
                _repository.Update(dbValue);
            }

            ClearCache();
        }

        public void SetValue(string key, Enum value)
        {
            SetValue(key, value.ToString().ToLower());
        }

        private void EnsureCache()
        {
            lock (_cache)
            {
                if (!_cache.Any())
                {
                    _cache = All().ToDictionary(c => c.Key.ToLower(), c => c.Value);
                }
            }
        }

        public static void ClearCache()
        {
            lock (_cache)
            {
                _cache = new Dictionary<string, string>();
            }
        }
    }
}
