using System;
using System.Collections.Generic;
using NzbDrone.Core.Download;
using NzbDrone.Core.Model;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Download.Clients.Sabnzbd;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigService
    {
        IEnumerable<Config> All();
        Dictionary<String, Object> AllWithDefaults();
        String NzbsOrgUId { get; set; }
        String NzbsOrgHash { get; set; }
        String NzbsrusUId { get; set; }
        String NzbsrusHash { get; set; }
        String FileSharingTalkUid { get; set; }
        String FileSharingTalkSecret { get; set; }
        String SabHost { get; set; }
        int SabPort { get; set; }
        String SabApiKey { get; set; }
        String SabUsername { get; set; }
        String SabPassword { get; set; }
        String SabTvCategory { get; set; }
        SabPriorityType SabBacklogTvPriority { get; set; }
        SabPriorityType SabRecentTvPriority { get; set; }
        String DownloadClientTvDirectory { get; set; }
        bool UseSeasonFolder { get; set; }
        string SortingSeasonFolderFormat { get; set; }
        int DefaultQualityProfile { get; set; }
        Boolean XbmcUpdateLibrary { get; set; }
        Boolean XbmcCleanLibrary { get; set; }
        Boolean XbmcUpdateWhenPlaying { get; set; }
        string XbmcHosts { get; set; }
        string XbmcUsername { get; set; }
        string XbmcPassword { get; set; }
        string UpdateUrl { get; set; }
        string SmtpServer { get; set; }
        int SmtpPort { get; set; }
        Boolean SmtpUseSsl { get; set; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }
        string SmtpFromAddress { get; set; }
        string SmtpToAddresses { get; set; }
        string TwitterAccessToken { get; set; }
        string TwitterAccessTokenSecret { get; set; }
        string GrowlHost { get; set; }
        string GrowlPassword { get; set; }
        string ProwlApiKeys { get; set; }
        int ProwlPriority { get; set; }
        bool EnableBacklogSearching { get; set; }
        bool AutoIgnorePreviouslyDownloadedEpisodes { get; set; }
        int Retention { get; set; }
        Guid UGuid { get; }
        DownloadClientType DownloadClient { get; set; }
        string BlackholeDirectory { get; set; }
        string ServiceRootUrl { get; }
        Boolean PlexUpdateLibrary { get; set; }
        string PlexServerHost { get; set; }
        string PlexClientHosts { get; set; }
        string PlexUsername { get; set; }
        string PlexPassword { get; set; }
        Boolean MetadataUseBanners { get; set; }
        string PneumaticDirectory { get; set; }
        string RecycleBin { get; set; }
        int RssSyncInterval { get; set; }
        string OmgwtfnzbsUsername { get; set; }
        string OmgwtfnzbsApiKey { get; set; }
        Boolean IgnoreArticlesWhenSortingSeries { get; set; }
        Boolean DownloadClientUseSceneName { get; set; }
        String NzbgetUsername { get; set; }
        String NzbgetPassword { get; set; }
        String NzbgetHost { get; set; }
        Int32 NzbgetPort { get; set; }
        String NzbgetTvCategory { get; set; }
        Int32 NzbgetPriority { get; set; }
        PriorityType NzbgetBacklogTvPriority { get; set; }
        PriorityType NzbgetRecentTvPriority { get; set; }
        string NzbRestrictions { get; set; }
        string GetValue(string key, object defaultValue, bool persist = false);
        void SetValue(string key, string value);
        void SaveValues(Dictionary<string, object> configValues);
    }
}