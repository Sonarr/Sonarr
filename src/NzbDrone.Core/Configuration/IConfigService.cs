using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigService
    {
        IEnumerable<Config> All();
        Dictionary<String, Object> AllWithDefaults();
        String DownloadedEpisodesFolder { get; set; }
        bool AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        int Retention { get; set; }
        string RecycleBin { get; set; }
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
        String ChownUser { get; set; }
        String ChownGroup { get; set; }
    }
}
