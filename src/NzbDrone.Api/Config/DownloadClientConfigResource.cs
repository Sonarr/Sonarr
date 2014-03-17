using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Config
{
    public class DownloadClientConfigResource : RestResource
    {
        public String DownloadedEpisodesFolder { get; set; }
        public String DownloadClientWorkingFolders { get; set; }
        public Int32 DownloadedEpisodesScanInterval { get; set; }

        public Boolean AutoRedownloadFailed { get; set; }
        public Boolean RemoveFailedDownloads { get; set; }
        public Boolean EnableFailedDownloadHandling { get; set; }
    }
}
