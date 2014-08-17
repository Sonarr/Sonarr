using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Update;


namespace NzbDrone.Core.Configuration
{
    public enum ConfigKey
    {
        DownloadedEpisodesFolder
    }

    public class ConfigService : IConfigService
    {
        private readonly IConfigRepository _repository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;
        private static Dictionary<string, string> _cache;

        public ConfigService(IConfigRepository repository, IEventAggregator eventAggregator, Logger logger)
        {
            _repository = repository;
            _eventAggregator = eventAggregator;
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

        public void SaveConfigDictionary(Dictionary<string, object> configValues)
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

            _eventAggregator.PublishEvent(new ConfigSavedEvent());
        }

        public Boolean IsDefined(String key)
        {
            return _repository.Get(key.ToLower()) != null;
        }

        public String DownloadedEpisodesFolder
        {
            get { return GetValue(ConfigKey.DownloadedEpisodesFolder.ToString()); }

            set { SetValue(ConfigKey.DownloadedEpisodesFolder.ToString(), value); }
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

        public string RecycleBin
        {
            get { return GetValue("RecycleBin", String.Empty); }
            set { SetValue("RecycleBin", value); }
        }

        public string ReleaseRestrictions
        {
            get { return GetValue("ReleaseRestrictions", String.Empty).Trim('\r', '\n'); }
            set { SetValue("ReleaseRestrictions", value.Trim('\r', '\n')); }
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

        public Boolean EnableCompletedDownloadHandling
        {
            get { return GetValueBoolean("EnableCompletedDownloadHandling", false); }

            set { SetValue("EnableCompletedDownloadHandling", value); }
        }

        public Boolean RemoveCompletedDownloads
        {
            get { return GetValueBoolean("RemoveCompletedDownloads", false); }

            set { SetValue("RemoveCompletedDownloads", value); }
        }

        public Boolean EnableFailedDownloadHandling
        {
            get { return GetValueBoolean("EnableFailedDownloadHandling", true); }

            set { SetValue("EnableFailedDownloadHandling", value); }
        }

        public Boolean AutoRedownloadFailed
        {
            get { return GetValueBoolean("AutoRedownloadFailed", true); }

            set { SetValue("AutoRedownloadFailed", value); }
        }

        public Boolean RemoveFailedDownloads
        {
            get { return GetValueBoolean("RemoveFailedDownloads", true); }

            set { SetValue("RemoveFailedDownloads", value); }
        }

        public Int32 BlacklistGracePeriod
        {
            get { return GetValueInt("BlacklistGracePeriod", 2); }

            set { SetValue("BlacklistGracePeriod", value); }
        }

        public Int32 BlacklistRetryInterval
        {
            get { return GetValueInt("BlacklistRetryInterval", 60); }

            set { SetValue("BlacklistRetryInterval", value); }
        }

        public Int32 BlacklistRetryLimit
        {
            get { return GetValueInt("BlacklistRetryLimit", 1); }

            set { SetValue("BlacklistRetryLimit", value); }
        }

        public Boolean CreateEmptySeriesFolders
        {
            get { return GetValueBoolean("CreateEmptySeriesFolders", false); }

            set { SetValue("CreateEmptySeriesFolders", value); }
        }

        public FileDateType FileDate
        {
            get { return GetValueEnum("FileDate", FileDateType.None); }

            set { SetValue("FileDate", value); }
        }

        public String DownloadClientWorkingFolders
        {
            get { return GetValue("DownloadClientWorkingFolders", "_UNPACK_|_FAILED_"); }
            set { SetValue("DownloadClientWorkingFolders", value); }
        }

        public Int32 DownloadedEpisodesScanInterval
        {
            get { return GetValueInt("DownloadedEpisodesScanInterval", 1); }

            set { SetValue("DownloadedEpisodesScanInterval", value); }
        }

        public Int32 DownloadClientHistoryLimit
        {
            get { return GetValueInt("DownloadClientHistoryLimit", 30); }

            set { SetValue("DownloadClientHistoryLimit", value); }
        }

        public Boolean SkipFreeSpaceCheckWhenImporting
        {
            get { return GetValueBoolean("SkipFreeSpaceCheckWhenImporting", false); }

            set { SetValue("SkipFreeSpaceCheckWhenImporting", value); }
        }

        public Boolean CopyUsingHardlinks
        {
            get { return GetValueBoolean("CopyUsingHardlinks", false); }

            set { SetValue("CopyUsingHardlinks", value); }
        }

        public Boolean SetPermissionsLinux
        {
            get { return GetValueBoolean("SetPermissionsLinux", false); }

            set { SetValue("SetPermissionsLinux", value); }
        }

        public String FileChmod
        {
            get { return GetValue("FileChmod", "0644"); }

            set { SetValue("FileChmod", value); }
        }

        public String FolderChmod
        {
            get { return GetValue("FolderChmod", "0755"); }

            set { SetValue("FolderChmod", value); }
        }

        public String ChownUser
        {
            get { return GetValue("ChownUser", ""); }

            set { SetValue("ChownUser", value); }
        }

        public String ChownGroup
        {
            get { return GetValue("ChownGroup", ""); }

            set { SetValue("ChownGroup", value); }
        }

        public Int32 FirstDayOfWeek
        {
            get { return GetValueInt("FirstDayOfWeek", (int)OsInfo.FirstDayOfWeek); }

            set { SetValue("FirstDayOfWeek", value); }
        }

        public String CalendarWeekColumnHeader
        {
            get { return GetValue("CalendarWeekColumnHeader", "ddd M/D"); }

            set { SetValue("CalendarWeekColumnHeader", value); }
        }

        public String ShortDateFormat
        {
            get { return GetValue("ShortDateFormat", "MMM D YYYY"); }

            set { SetValue("ShortDateFormat", value); }
        }

        public String LongDateFormat
        {
            get { return GetValue("LongDateFormat", "dddd, MMMM D YYYY"); }

            set { SetValue("LongDateFormat", value); }
        }

        public String TimeFormat
        {
            get { return GetValue("TimeFormat", "h(:mm)a"); }

            set { SetValue("TimeFormat", value); }
        }

        public Boolean ShowRelativeDates
        {
            get { return GetValueBoolean("ShowRelativeDates", true); }

            set { SetValue("ShowRelativeDates", value); }
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
            key = key.ToLowerInvariant();
            Ensure.That(key, () => key).IsNotNullOrWhiteSpace();

            EnsureCache();

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

            _logger.Trace("Writing Setting to database. Key:'{0}' Value:'{1}'", key, value);

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
