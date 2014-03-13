using System;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigService
    {
        IEnumerable<Config> All();
        Dictionary<String, Object> AllWithDefaults();
        void SaveConfigDictionary(Dictionary<string, object> configValues);

        //Download Client
        String DownloadedEpisodesFolder { get; set; }
        String DownloadClientWorkingFolders { get; set; }

        //Failed Download Handling (Download client)
        Boolean AutoRedownloadFailed { get; set; }
        Boolean RemoveFailedDownloads { get; set; }
        Boolean EnableFailedDownloadHandling { get; set; }

        //Media Management
        Boolean AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        String RecycleBin { get; set; }
        Boolean AutoDownloadPropers { get; set; }
        Boolean CreateEmptySeriesFolders { get; set; }
        FileDateType FileDate { get; set; }

        //Permissions (Media Management)
        Boolean SetPermissionsLinux { get; set; }
        String FileChmod { get; set; }
        String FolderChmod { get; set; }
        String ChownUser { get; set; }
        String ChownGroup { get; set; }

        //Indexers
        Int32 Retention { get; set; }
        Int32 RssSyncInterval { get; set; }
        String ReleaseRestrictions { get; set; }
    }
}
