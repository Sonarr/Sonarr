using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Config
{
    public class MediaManagementConfigResource : RestResource
    {
        public bool AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        public string RecycleBin { get; set; }
        public bool AutoDownloadPropers { get; set; }
        public bool CreateEmptySeriesFolders { get; set; }
        public FileDateType FileDate { get; set; }

        public bool SetPermissionsLinux { get; set; }
        public string FileChmod { get; set; }
        public string FolderChmod { get; set; }
        public string ChownUser { get; set; }
        public string ChownGroup { get; set; }

        public bool SkipFreeSpaceCheckWhenImporting { get; set; }
        public bool CopyUsingHardlinks { get; set; }
        public bool ImportExtraFiles { get; set; }
        public string ExtraFileExtensions { get; set; }
        public bool EnableMediaInfo { get; set; }
    }

    public static class MediaManagementConfigResourceMapper
    {
        public static MediaManagementConfigResource ToResource(IConfigService model)
        {
            return new MediaManagementConfigResource
            {
                AutoUnmonitorPreviouslyDownloadedEpisodes = model.AutoUnmonitorPreviouslyDownloadedEpisodes,
                RecycleBin = model.RecycleBin,
                AutoDownloadPropers = model.AutoDownloadPropers,
                CreateEmptySeriesFolders = model.CreateEmptySeriesFolders,
                FileDate = model.FileDate,

                SetPermissionsLinux = model.SetPermissionsLinux,
                FileChmod = model.FileChmod,
                FolderChmod = model.FolderChmod,
                ChownUser = model.ChownUser,
                ChownGroup = model.ChownGroup,

                SkipFreeSpaceCheckWhenImporting = model.SkipFreeSpaceCheckWhenImporting,
                CopyUsingHardlinks = model.CopyUsingHardlinks,
                ImportExtraFiles = model.ImportExtraFiles,
                ExtraFileExtensions = model.ExtraFileExtensions,
                EnableMediaInfo = model.EnableMediaInfo
            };
        }
    }
}
