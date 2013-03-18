using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Configuration
{
    public class ConfigService : IConfigService
    {
        private readonly IConfigRepository _repository;
        private readonly Logger _logger;
        private static Dictionary<string, string> _cache;

        public ConfigService(IConfigRepository repository, Logger logger)
        {
            _repository = repository;
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

            foreach(var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(this, null);
                
                dict.Add(propertyInfo.Name, value);
            }

            return dict;
        }

        public String NzbsOrgUId
        {
            get { return GetValue("NzbsOrgUId"); }

            set { SetValue("NzbsOrgUId", value); }
        }

        public String NzbsOrgHash
        {
            get { return GetValue("NzbsOrgHash"); }

            set { SetValue("NzbsOrgHash", value); }
        }

        public String NzbsrusUId
        {
            get { return GetValue("NzbsrusUId"); }

            set { SetValue("NzbsrusUId", value); }
        }

        public String NzbsrusHash
        {
            get { return GetValue("NzbsrusHash"); }

            set { SetValue("NzbsrusHash", value); }
        }

        public String FileSharingTalkUid
        {
            get { return GetValue("FileSharingTalkUid"); }

            set { SetValue("FileSharingTalkUid", value); }
        }

        public String FileSharingTalkSecret
        {
            get { return GetValue("FileSharingTalkSecret"); }

            set { SetValue("FileSharingTalkSecret", value); }
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

        public SabPriorityType SabBacklogTvPriority
        {
            get { return (SabPriorityType)GetValueInt("SabBacklogTvPriority"); }

            set { SetValue("SabBacklogTvPriority", (int)value); }
        }

        public SabPriorityType SabRecentTvPriority
        {
            get { return (SabPriorityType)GetValueInt("SabRecentTvPriority"); }

            set { SetValue("SabRecentTvPriority", (int)value); }
        }

        public String DownloadClientTvDirectory
        {
            get { return GetValue("DownloadClientTvDirectory"); }

            set { SetValue("DownloadClientTvDirectory", value); }
        }

        public bool UseSeasonFolder
        {
            get { return GetValueBoolean("UseSeasonFolder", true); }

            set { SetValue("UseSeasonFolder", value); }
        }

        public string SortingSeasonFolderFormat
        {
            get { return GetValue("Sorting_SeasonFolderFormat", "Season %s"); }
            set { SetValue("Sorting_SeasonFolderFormat", value); }
        }

        public int DefaultQualityProfile
        {
            get { return GetValueInt("DefaultQualityProfile", 1); }

            set { SetValue("DefaultQualityProfile", value); }
        }

        public Boolean XbmcUpdateLibrary
        {
            get { return GetValueBoolean("XbmcUpdateLibrary"); }

            set { SetValue("XbmcUpdateLibrary", value); }
        }

        public Boolean XbmcCleanLibrary
        {
            get { return GetValueBoolean("XbmcCleanLibrary"); }

            set { SetValue("XbmcCleanLibrary", value); }
        }

        public Boolean XbmcUpdateWhenPlaying
        {
            get { return GetValueBoolean("XbmcUpdateWhenPlaying"); }

            set { SetValue("XbmcUpdateWhenPlaying", value); }
        }

        public string XbmcHosts
        {
            get { return GetValue("XbmcHosts", "localhost:8080"); }
            set { SetValue("XbmcHosts", value); }
        }

        public string XbmcUsername
        {
            get { return GetValue("XbmcUsername", "xbmc"); }
            set { SetValue("XbmcUsername", value); }
        }

        public string XbmcPassword
        {
            get { return GetValue("XbmcPassword", String.Empty); }
            set { SetValue("XbmcPassword", value); }
        }

        public string UpdateUrl
        {
            get { return GetValue("UpdateUrl", UpdateProvider.DEFAULT_UPDATE_URL); }
            set { SetValue("UpdateUrl", value); }
        }

        public string SmtpServer
        {
            get { return GetValue("SmtpServer", String.Empty); }
            set { SetValue("SmtpServer", value); }
        }

        public int SmtpPort
        {
            get { return GetValueInt("SmtpPort", 25); }
            set { SetValue("SmtpPort", value); }
        }

        public Boolean SmtpUseSsl
        {
            get { return GetValueBoolean("SmtpUseSsl"); }

            set { SetValue("SmtpUseSsl", value); }
        }

        public string SmtpUsername
        {
            get { return GetValue("SmtpUsername", String.Empty); }
            set { SetValue("SmtpUsername", value); }
        }

        public string SmtpPassword
        {
            get { return GetValue("SmtpPassword", String.Empty); }
            set { SetValue("SmtpPassword", value); }
        }

        public string SmtpFromAddress
        {
            get { return GetValue("SmtpFromAddress", String.Empty); }
            set { SetValue("SmtpFromAddress", value); }
        }

        public string SmtpToAddresses
        {
            get { return GetValue("SmtpToAddresses", String.Empty); }
            set { SetValue("SmtpToAddresses", value); }
        }

        public string TwitterAccessToken
        {
            get { return GetValue("TwitterAccessToken", String.Empty); }
            set { SetValue("TwitterAccessToken", value); }
        }

        public string TwitterAccessTokenSecret
        {
            get { return GetValue("TwitterAccessTokenSecret", String.Empty); }
            set { SetValue("TwitterAccessTokenSecret", value); }
        }
       
        public string GrowlHost
        {
            get { return GetValue("GrowlHost", "localhost:23053"); }
            set { SetValue("GrowlHost", value); }
        }

        public string GrowlPassword
        {
            get { return GetValue("GrowlPassword", String.Empty); }
            set { SetValue("GrowlPassword", value); }
        }
       
        public string ProwlApiKeys
        {
            get { return GetValue("ProwlApiKeys", String.Empty); }
            set { SetValue("ProwlApiKeys", value); }
        }

        public int ProwlPriority
        {
            get { return GetValueInt("ProwlPriority", 0); }
            set { SetValue("ProwlPriority", value); }
        }

        public bool EnableBacklogSearching
        {
            get { return GetValueBoolean("EnableBacklogSearching"); }
            set { SetValue("EnableBacklogSearching", value); }
        }

        public bool AutoIgnorePreviouslyDownloadedEpisodes
        {
            get { return GetValueBoolean("AutoIgnorePreviouslyDownloadedEpisodes"); }
            set { SetValue("AutoIgnorePreviouslyDownloadedEpisodes", value); }
        }

        public int Retention
        {
            get { return GetValueInt("Retention", 0); }
            set { SetValue("Retention", value); }
        }

        public Guid UGuid
        {
            get { return Guid.Parse(GetValue("UGuid", Guid.NewGuid().ToString(), persist: true)); }
        }

        public DownloadClientType DownloadClient
        {
            get { return (DownloadClientType)GetValueInt("DownloadClient"); }

            set { SetValue("DownloadClient", (int)value); }
        }

        public string BlackholeDirectory
        {
            get { return GetValue("BlackholeDirectory", String.Empty); }
            set { SetValue("BlackholeDirectory", value); }
        }

        public string ServiceRootUrl
        {
            get { return "http://services.nzbdrone.com"; }
        }

        public Boolean PlexUpdateLibrary
        {
            get { return GetValueBoolean("PlexUpdateLibrary"); }

            set { SetValue("PlexUpdateLibrary", value); }
        }

        public string PlexServerHost
        {
            get { return GetValue("PlexServerHost", "localhost:32400"); }
            set { SetValue("PlexServerHost", value); }
        }

        public string PlexClientHosts
        {
            get { return GetValue("PlexClientHosts", "localhost:3000"); }
            set { SetValue("PlexClientHosts", value); }
        }

        public string PlexUsername
        {
            get { return GetValue("PlexUsername"); }
            set { SetValue("PlexUsername", value); }
        }

        public string PlexPassword
        {
            get { return GetValue("PlexPassword"); }
            set { SetValue("PlexPassword", value); }
        }

        public Boolean MetadataUseBanners
        {
            get { return GetValueBoolean("MetadataUseBanners"); }

            set { SetValue("MetadataUseBanners", value); }
        }

        public string PneumaticDirectory
        {
            get { return GetValue("PneumaticDirectory", String.Empty); }
            set { SetValue("PneumaticDirectory", value); }
        }

        public string RecycleBin
        {
            get { return GetValue("RecycleBin", String.Empty); }
            set { SetValue("RecycleBin", value); }
        }

        public int RssSyncInterval
        {
            get { return GetValueInt("RssSyncInterval", 25); }
            set { SetValue("RssSyncInterval", value); }
        }

        public string OmgwtfnzbsUsername
        {
            get { return GetValue("OmgwtfnzbsUsername", String.Empty); }
            set { SetValue("OmgwtfnzbsUsername", value); }
        }

        public string OmgwtfnzbsApiKey
        {
            get { return GetValue("OmgwtfnzbsApiKey", String.Empty); }
            set { SetValue("OmgwtfnzbsApiKey", value); }
        }

        public Boolean IgnoreArticlesWhenSortingSeries
        {
            get { return GetValueBoolean("IgnoreArticlesWhenSortingSeries", true); }

            set { SetValue("IgnoreArticlesWhenSortingSeries", value); }
        }

        public Boolean DownloadClientUseSceneName
        {
            get { return GetValueBoolean("DownloadClientUseSceneName", false); }

            set { SetValue("DownloadClientUseSceneName", value); }
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

        public Int32 NzbgetPriority
        {
            get { return GetValueInt("NzbgetPriority", 0); }

            set { SetValue("NzbgetPriority", value); }
        }

        public PriorityType NzbgetBacklogTvPriority
        {
            get { return (PriorityType)GetValueInt("NzbgetBacklogTvPriority"); }

            set { SetValue("NzbgetBacklogTvPriority", (int)value); }
        }

        public PriorityType NzbgetRecentTvPriority
        {
            get { return (PriorityType)GetValueInt("NzbgetRecentTvPriority"); }

            set { SetValue("NzbgetRecentTvPriority", (int)value); }
        }

        public string NzbRestrictions
        {
            get { return GetValue("NzbRestrictions", String.Empty); }
            set { SetValue("NzbRestrictions", value); }
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

        public void SaveValues(Dictionary<string, object> configValues)
        {
            var allWithDefaults = AllWithDefaults();

            foreach(var configValue in configValues)
            {
                object currentValue;
                allWithDefaults.TryGetValue(configValue.Key, out currentValue);
                if (currentValue == null) continue;

                var equal = configValue.Value.ToString().Equals(currentValue.ToString());

                if (!equal)
                    SetValue(configValue.Key, configValue.Value.ToString());
            }
        }

        private void EnsureCache()
        {
            lock (_cache)
            {
                if (!_cache.Any())
                {
                    _cache = All().ToDictionary(c => c.Key, c => c.Value);
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
