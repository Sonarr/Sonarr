using System;
using System.Collections.Generic;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Download.Clients.Sabnzbd;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigService
    {
        IEnumerable<Config> All();
        Dictionary<String, Object> AllWithDefaults();
        String SabHost { get; set; }
        int SabPort { get; set; }
        String SabApiKey { get; set; }
        String SabUsername { get; set; }
        String SabPassword { get; set; }
        String SabTvCategory { get; set; }
        SabPriorityType SabRecentTvPriority { get; set; }
        SabPriorityType SabOlderTvPriority { get; set; }
        Boolean SabUseSsl { get; set; }
        String DownloadedEpisodesFolder { get; set; }
        bool AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        int Retention { get; set; }
        DownloadClientType DownloadClient { get; set; }
        string BlackholeFolder { get; set; }
        string PneumaticFolder { get; set; }
        string RecycleBin { get; set; }
        String NzbgetUsername { get; set; }
        String NzbgetPassword { get; set; }
        String NzbgetHost { get; set; }
        Int32 NzbgetPort { get; set; }
        String NzbgetTvCategory { get; set; }
        PriorityType NzbgetRecentTvPriority { get; set; }
        PriorityType NzbgetOlderTvPriority { get; set; }
        string ReleaseRestrictions { get; set; }
        Int32 RssSyncInterval { get; set; }
        Boolean AutoDownloadPropers { get; set; }
        String DownloadClientWorkingFolders { get; set; }
        Boolean AutoRedownloadFailed { get; set; }
        Boolean RemoveFailedDownloads { get; set; }
        Boolean EnableFailedDownloadHandling { get; set; }
        Boolean CreateEmptySeriesFolders { get; set; }
        void SaveValues(Dictionary<string, object> configValues);
        Boolean SetPermissionsLinux { get; set; }
        String FileChmod { get; set; }
        String FolderChmod { get; set; }
    }
}
