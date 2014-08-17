using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Api.Config
{
    public class MediaManagementConfigResource : RestResource
    {
        public Boolean AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        public String RecycleBin { get; set; }
        public Boolean AutoDownloadPropers { get; set; }
        public Boolean CreateEmptySeriesFolders { get; set; }
        public FileDateType FileDate { get; set; }

        public Boolean SetPermissionsLinux { get; set; }
        public String FileChmod { get; set; }
        public String FolderChmod { get; set; }
        public String ChownUser { get; set; }
        public String ChownGroup { get; set; }

        public Boolean SkipFreeSpaceCheckWhenImporting { get; set; }
        public Boolean CopyUsingHardlinks { get; set; }
    }
}
