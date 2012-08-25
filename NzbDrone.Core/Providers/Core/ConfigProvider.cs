using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Sabnzbd;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers.Core
{
    public class ConfigProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static Dictionary<string, string> cache = new Dictionary<string, string>();

        private readonly IDatabase _database;

        [Inject]
        public ConfigProvider(IDatabase database)
        {
            _database = database;
        }

        public ConfigProvider()
        {
        }

        public IEnumerable<Config> All()
        {
            return _database.Fetch<Config>();
        }

        public virtual String NzbMatrixUsername
        {
            get { return GetValue("NzbMatrixUsername"); }

            set { SetValue("NzbMatrixUsername", value); }
        }

        public virtual String NzbMatrixApiKey
        {
            get { return GetValue("NzbMatrixApiKey"); }

            set { SetValue("NzbMatrixApiKey", value); }
        }

        public virtual String NzbsOrgUId
        {
            get { return GetValue("NzbsOrgUId"); }

            set { SetValue("NzbsOrgUId", value); }
        }

        public virtual String NzbsOrgHash
        {
            get { return GetValue("NzbsOrgHash"); }

            set { SetValue("NzbsOrgHash", value); }
        }

        public virtual String NzbsrusUId
        {
            get { return GetValue("NzbsrusUId"); }

            set { SetValue("NzbsrusUId", value); }
        }

        public virtual String NzbsrusHash
        {
            get { return GetValue("NzbsrusHash"); }

            set { SetValue("NzbsrusHash", value); }
        }

        public virtual String NewzbinUsername
        {
            get { return GetValue("NewzbinUsername"); }

            set { SetValue("NewzbinUsername", value); }
        }

        public virtual String NewzbinPassword
        {
            get { return GetValue("NewzbinPassword"); }

            set { SetValue("NewzbinPassword", value); }
        }

        public virtual String FileSharingTalkUid
        {
            get { return GetValue("FileSharingTalkUid"); }

            set { SetValue("FileSharingTalkUid", value); }
        }

        public virtual String FileSharingTalkSecret
        {
            get { return GetValue("FileSharingTalkSecret"); }

            set { SetValue("FileSharingTalkSecret", value); }
        }

        public virtual String SabHost
        {
            get { return GetValue("SabHost", "localhost"); }

            set { SetValue("SabHost", value); }
        }

        public virtual int SabPort
        {
            get { return GetValueInt("SabPort", 8080); }

            set { SetValue("SabPort", value); }
        }

        public virtual String SabApiKey
        {
            get { return GetValue("SabApiKey"); }

            set { SetValue("SabApiKey", value); }
        }

        public virtual String SabUsername
        {
            get { return GetValue("SabUsername"); }

            set { SetValue("SabUsername", value); }
        }

        public virtual String SabPassword
        {
            get { return GetValue("SabPassword"); }

            set { SetValue("SabPassword", value); }
        }

        public virtual String SabTvCategory
        {
            get { return GetValue("SabTvCategory", "tv"); }

            set { SetValue("SabTvCategory", value); }
        }

        public virtual SabPriorityType SabTvPriority
        {
            get { return (SabPriorityType)GetValueInt("SabTvPriority"); }

            set { SetValue("SabTvPriority", (int)value); }
        }

        public virtual String SabDropDirectory
        {
            get { return GetValue("SabTvDropDirectory"); }

            set { SetValue("SabTvDropDirectory", value); }
        }

        public virtual bool SortingIncludeSeriesName
        {
            get { return GetValueBoolean("Sorting_SeriesName", true); }
            set { SetValue("Sorting_SeriesName", value); }
        }

        public virtual bool SortingIncludeEpisodeTitle
        {
            get { return GetValueBoolean("Sorting_EpisodeName", true); }
            set { SetValue("Sorting_EpisodeName", value); }
        }

        public virtual bool SortingReplaceSpaces
        {
            get { return GetValueBoolean("Sorting_ReplaceSpaces"); }
            set { SetValue("Sorting_ReplaceSpaces", value); }
        }

        public virtual bool SortingAppendQuality
        {
            get { return GetValueBoolean("Sorting_AppendQaulity", true); }
            set { SetValue("Sorting_AppendQaulity", value); }
        }

        public virtual bool UseSeasonFolder
        {
            get { return GetValueBoolean("UseSeasonFolder", true); }

            set { SetValue("UseSeasonFolder", value); }
        }

        public virtual string SortingSeasonFolderFormat
        {
            get { return GetValue("Sorting_SeasonFolderFormat", "Season %s"); }
            set { SetValue("Sorting_SeasonFolderFormat", value); }
        }

        public virtual int SortingSeparatorStyle
        {
            get { return GetValueInt("Sorting_SeparatorStyle"); }
            set { SetValue("Sorting_SeparatorStyle", value); }
        }

        public virtual int SortingNumberStyle
        {
            get { return GetValueInt("Sorting_NumberStyle", 2); }
            set { SetValue("Sorting_NumberStyle", value); }
        }

        public virtual int SortingMultiEpisodeStyle
        {
            get { return GetValueInt("Sorting_MultiEpisodeStyle"); }
            set { SetValue("Sorting_MultiEpisodeStyle", value); }
        }

        public virtual bool SortingUseSceneName
        {
            get { return GetValueBoolean("Sorting_UseSceneName", false); }
            set { SetValue("Sorting_UseSceneName", value); }
        }

        public virtual int DefaultQualityProfile
        {
            get { return GetValueInt("DefaultQualityProfile", 1); }

            set { SetValue("DefaultQualityProfile", value); }
        }

        public virtual Boolean XbmcNotifyOnGrab
        {
            get { return GetValueBoolean("XbmcNotifyOnGrab"); }

            set { SetValue("XbmcNotifyOnGrab", value); }
        }

        public virtual Boolean XbmcNotifyOnDownload
        {
            get { return GetValueBoolean("XbmcNotifyOnDownload"); }

            set { SetValue("XbmcNotifyOnDownload", value); }
        }

        public virtual Boolean XbmcUpdateLibrary
        {
            get { return GetValueBoolean("XbmcUpdateLibrary"); }

            set { SetValue("XbmcUpdateLibrary", value); }
        }

        public virtual Boolean XbmcCleanLibrary
        {
            get { return GetValueBoolean("XbmcCleanLibrary"); }

            set { SetValue("XbmcCleanLibrary", value); }
        }

        public virtual Boolean XbmcUpdateWhenPlaying
        {
            get { return GetValueBoolean("XbmcUpdateWhenPlaying"); }

            set { SetValue("XbmcUpdateWhenPlaying", value); }
        }

        public virtual string XbmcHosts
        {
            get { return GetValue("XbmcHosts", "localhost:8080"); }
            set { SetValue("XbmcHosts", value); }
        }

        public virtual string XbmcUsername
        {
            get { return GetValue("XbmcUsername", "xbmc"); }
            set { SetValue("XbmcUsername", value); }
        }

        public virtual string XbmcPassword
        {
            get { return GetValue("XbmcPassword", String.Empty); }
            set { SetValue("XbmcPassword", value); }
        }

        public virtual string UpdateUrl
        {
            get { return GetValue("UpdateUrl", UpdateProvider.DEFAULT_UPDATE_URL); }
            set { SetValue("UpdateUrl", value); }
        }

        public virtual Boolean SmtpNotifyOnGrab
        {
            get { return GetValueBoolean("SmtpNotifyOnGrab"); }

            set { SetValue("SmtpNotifyOnGrab", value); }
        }

        public virtual Boolean SmtpNotifyOnDownload
        {
            get { return GetValueBoolean("SmtpNotifyOnDownload"); }

            set { SetValue("SmtpNotifyOnDownload", value); }
        }

        public virtual string SmtpServer
        {
            get { return GetValue("SmtpServer", String.Empty); }
            set { SetValue("SmtpServer", value); }
        }

        public virtual int SmtpPort
        {
            get { return GetValueInt("SmtpPort", 25); }
            set { SetValue("SmtpPort", value); }
        }

        public virtual Boolean SmtpUseSsl
        {
            get { return GetValueBoolean("SmtpUseSsl"); }

            set { SetValue("SmtpUseSsl", value); }
        }

        public virtual string SmtpUsername
        {
            get { return GetValue("SmtpUsername", String.Empty); }
            set { SetValue("SmtpUsername", value); }
        }

        public virtual string SmtpPassword
        {
            get { return GetValue("SmtpPassword", String.Empty); }
            set { SetValue("SmtpPassword", value); }
        }

        public virtual string SmtpFromAddress
        {
            get { return GetValue("SmtpFromAddress", String.Empty); }
            set { SetValue("SmtpFromAddress", value); }
        }

        public virtual string SmtpToAddresses
        {
            get { return GetValue("SmtpToAddresses", String.Empty); }
            set { SetValue("SmtpToAddresses", value); }
        }

        public virtual Boolean TwitterNotifyOnGrab
        {
            get { return GetValueBoolean("TwitterNotifyOnGrab"); }

            set { SetValue("TwitterNotifyOnGrab", value); }
        }

        public virtual Boolean TwitterNotifyOnDownload
        {
            get { return GetValueBoolean("TwitterNotifyOnDownload"); }

            set { SetValue("TwitterNotifyOnDownload", value); }
        }

        public virtual string TwitterAccessToken
        {
            get { return GetValue("TwitterAccessToken", String.Empty); }
            set { SetValue("TwitterAccessToken", value); }
        }

        public virtual string TwitterAccessTokenSecret
        {
            get { return GetValue("TwitterAccessTokenSecret", String.Empty); }
            set { SetValue("TwitterAccessTokenSecret", value); }
        }

        public virtual Boolean GrowlNotifyOnGrab
        {
            get { return GetValueBoolean("GrowlNotifyOnGrab"); }

            set { SetValue("GrowlNotifyOnGrab", value); }
        }

        public virtual Boolean GrowlNotifyOnDownload
        {
            get { return GetValueBoolean("GrowlNotifyOnDownload"); }

            set { SetValue("GrowlNotifyOnDownload", value); }
        }

        public virtual string GrowlHost
        {
            get { return GetValue("GrowlHost", "localhost:23053"); }
            set { SetValue("GrowlHost", value); }
        }

        public virtual string GrowlPassword
        {
            get { return GetValue("GrowlPassword", String.Empty); }
            set { SetValue("GrowlPassword", value); }
        }

        public virtual Boolean ProwlNotifyOnGrab
        {
            get { return GetValueBoolean("ProwlNotifyOnGrab"); }

            set { SetValue("ProwlNotifyOnGrab", value); }
        }

        public virtual Boolean ProwlNotifyOnDownload
        {
            get { return GetValueBoolean("ProwlNotifyOnDownload"); }

            set { SetValue("ProwlNotifyOnDownload", value); }
        }

        public virtual string ProwlApiKeys
        {
            get { return GetValue("ProwlApiKeys", String.Empty); }
            set { SetValue("ProwlApiKeys", value); }
        }

        public virtual int ProwlPriority
        {
            get { return GetValueInt("ProwlPriority", 0); }
            set { SetValue("ProwlPriority", value); }
        }

        public virtual bool EnableBacklogSearching
        {
            get { return GetValueBoolean("EnableBacklogSearching"); }
            set { SetValue("EnableBacklogSearching", value); }
        }

        public virtual bool AutoIgnorePreviouslyDownloadedEpisodes
        {
            get { return GetValueBoolean("AutoIgnorePreviouslyDownloadedEpisodes"); }
            set { SetValue("AutoIgnorePreviouslyDownloadedEpisodes", value); }
        }

        public virtual int Retention
        {
            get { return GetValueInt("Retention", 0); }
            set { SetValue("Retention", value); }
        }

        public Guid UGuid
        {
            get { return Guid.Parse(GetValue("UGuid", Guid.NewGuid().ToString(), persist: true)); }
        }

        public virtual DownloadClientType DownloadClient
        {
            get { return (DownloadClientType)GetValueInt("DownloadClient"); }

            set { SetValue("DownloadClient", (int)value); }
        }

        public virtual string BlackholeDirectory
        {
            get { return GetValue("BlackholeDirectory", String.Empty); }
            set { SetValue("BlackholeDirectory", value); }
        }

        public virtual string ServiceRootUrl
        {
            get { return "http://services.nzbdrone.com"; }
        }

        public virtual Boolean PlexNotifyOnGrab
        {
            get { return GetValueBoolean("PlexNotifyOnGrab"); }

            set { SetValue("PlexNotifyOnGrab", value); }
        }

        public virtual Boolean PlexNotifyOnDownload
        {
            get { return GetValueBoolean("PlexNotifyOnDownload"); }

            set { SetValue("PlexNotifyOnDownload", value); }
        }

        public virtual Boolean PlexUpdateLibrary
        {
            get { return GetValueBoolean("PlexUpdateLibrary"); }

            set { SetValue("PlexUpdateLibrary", value); }
        }

        public virtual string PlexServerHost
        {
            get { return GetValue("PlexServerHost", "localhost:32400"); }
            set { SetValue("PlexServerHost", value); }
        }

        public virtual string PlexClientHosts
        {
            get { return GetValue("PlexClientHosts", "localhost:3000"); }
            set { SetValue("PlexClientHosts", value); }
        }

        public virtual string PlexUsername
        {
            get { return GetValue("PlexUsername"); }
            set { SetValue("PlexUsername", value); }
        }

        public virtual string PlexPassword
        {
            get { return GetValue("PlexPassword"); }
            set { SetValue("PlexPassword", value); }
        }

        public virtual Boolean MetadataUseBanners
        {
            get { return GetValueBoolean("MetadataUseBanners"); }

            set { SetValue("MetadataUseBanners", value); }
        }

        public virtual string AllowedReleaseGroups
        {
            get { return GetValue("AllowedReleaseGroups"); }
            set { SetValue("AllowedReleaseGroups", value); }
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
            return Convert.ToInt16(GetValue(key, defaultValue));
        }

        public virtual string GetValue(string key, object defaultValue, bool persist = false)
        {
            EnsureCache();


            string dbValue;

            if (cache.TryGetValue(key, out dbValue) && dbValue != null && !String.IsNullOrEmpty(dbValue))
                return dbValue;

            logger.Trace("Unable to find config key '{0}' defaultValue:'{1}'", key, defaultValue);

            if (persist)
                SetValue(key, defaultValue.ToString());

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
            if (String.IsNullOrEmpty(key))
                throw new ArgumentOutOfRangeException("key");
            if (value == null)
                throw new ArgumentNullException("key");

            logger.Trace("Writing Setting to file. Key:'{0}' Value:'{1}'", key, value);

            var dbValue = _database.SingleOrDefault<Config>("WHERE [KEY]=@0", key);

            if (dbValue == null)
            {
                _database.Insert(new Config { Key = key, Value = value });
            }
            else
            {
                dbValue.Value = value;
                _database.Update(dbValue);
            }

            ClearCache();
        }

        private void EnsureCache()
        {
            lock (cache)
            {
                if (!cache.Any())
                {
                    cache = _database.Fetch<Config>().ToDictionary(c => c.Key, c => c.Value);
                }
            }
        }

        public static void ClearCache()
        {
            lock (cache)
            {
                cache = new Dictionary<string, string>();
            }
        }
    }
}
