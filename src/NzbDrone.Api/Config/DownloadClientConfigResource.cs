using Sonarr.Http.REST;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class DownloadClientConfigResource : RestResource
    {
        public string DownloadedEpisodesFolder { get; set; }
        public string DownloadClientWorkingFolders { get; set; }
        public int DownloadedEpisodesScanInterval { get; set; }

        public bool EnableCompletedDownloadHandling { get; set; }
        public bool RemoveCompletedDownloads { get; set; }

        public bool AutoRedownloadFailed { get; set; }
        public bool RemoveFailedDownloads { get; set; }
    }

    public static class DownloadClientConfigResourceMapper
    {
        public static DownloadClientConfigResource ToResource(IConfigService model)
        {
            return new DownloadClientConfigResource
            {
                DownloadedEpisodesFolder = model.DownloadedEpisodesFolder,
                DownloadClientWorkingFolders = model.DownloadClientWorkingFolders,
                DownloadedEpisodesScanInterval = model.DownloadedEpisodesScanInterval,

                EnableCompletedDownloadHandling = model.EnableCompletedDownloadHandling,
                RemoveCompletedDownloads = model.RemoveCompletedDownloads,

                AutoRedownloadFailed = model.AutoRedownloadFailed,
                RemoveFailedDownloads = model.RemoveFailedDownloads
            };
        }
    }
}
