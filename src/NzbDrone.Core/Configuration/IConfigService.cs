using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Common.Http.Proxy;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigService
    {
        void SaveConfigDictionary(Dictionary<string, object> configValues);

        bool IsDefined(string key);

        //Download Client
        string DownloadClientWorkingFolders { get; set; }
        int DownloadClientHistoryLimit { get; set; }

        //Completed/Failed Download Handling (Download client)
        bool EnableCompletedDownloadHandling { get; set; }
        bool RemoveCompletedDownloads { get; set; }

        bool AutoRedownloadFailed { get; set; }
        bool RemoveFailedDownloads { get; set; }

        //Media Management
        bool AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        string RecycleBin { get; set; }
        bool AutoDownloadPropers { get; set; }
        bool CreateEmptySeriesFolders { get; set; }
        FileDateType FileDate { get; set; }
        bool SkipFreeSpaceCheckWhenImporting { get; set; }
        bool CopyUsingHardlinks { get; set; }
        bool EnableMediaInfo { get; set; }
        bool ImportExtraFiles { get; set; }
        string ExtraFileExtensions { get; set; }

        //Permissions (Media Management)
        bool SetPermissionsLinux { get; set; }
        string FileChmod { get; set; }
        string FolderChmod { get; set; }
        string ChownUser { get; set; }
        string ChownGroup { get; set; }

        //Indexers
        int Retention { get; set; }
        int RssSyncInterval { get; set; }
        int MinimumAge { get; set; }

        //UI
        int FirstDayOfWeek { get; set; }
        string CalendarWeekColumnHeader { get; set; }

        string ShortDateFormat { get; set; }
        string LongDateFormat { get; set; }
        string TimeFormat { get; set; }
        bool ShowRelativeDates { get; set; }
        bool EnableColorImpairedMode { get; set; }

        //Internal
        bool CleanupMetadataImages { get; set; }


        //Forms Auth
        string RijndaelPassphrase { get; }
        string HmacPassphrase { get; }
        string RijndaelSalt { get; }
        string HmacSalt { get; }

        //Proxy
        bool ProxyEnabled { get; }
        ProxyType ProxyType { get; }
        string ProxyHostname { get; }
        int ProxyPort { get; }
        string ProxyUsername { get; }
        string ProxyPassword { get; }
        string ProxyBypassFilter { get; }
        bool ProxyBypassLocalAddresses { get; }
    }
}
