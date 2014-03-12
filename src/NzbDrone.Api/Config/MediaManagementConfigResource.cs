using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Config
{
    public class MediaManagementConfigResource : RestResource
    {
        public Boolean AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        public Boolean FileDateAiredDate { get; set; }
        public String RecycleBin { get; set; }
        public Boolean AutoDownloadPropers { get; set; }
        public Boolean CreateEmptySeriesFolders { get; set; }

        public Boolean SetPermissionsLinux { get; set; }
        public String FileChmod { get; set; }
        public String FolderChmod { get; set; }
        public String ChownUser { get; set; }
        public String ChownGroup { get; set; }
    }
}
