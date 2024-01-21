using System;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookEpisodeFile
    {
        public WebhookEpisodeFile()
        {
        }

        public WebhookEpisodeFile(EpisodeFile episodeFile)
        {
            Id = episodeFile.Id;
            RelativePath = episodeFile.RelativePath;
            Path = episodeFile.Path;
            Quality = episodeFile.Quality.Quality.Name;
            QualityVersion = episodeFile.Quality.Revision.Version;
            ReleaseGroup = episodeFile.ReleaseGroup;
            SceneName = episodeFile.SceneName;
            Size = episodeFile.Size;
            DateAdded = episodeFile.DateAdded;

            if (episodeFile.MediaInfo != null)
            {
                MediaInfo = new WebhookEpisodeFileMediaInfo(episodeFile);
            }
        }

        public int Id { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string SceneName { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public WebhookEpisodeFileMediaInfo MediaInfo { get; set; }
        public string RecycleBinPath { get; set; }
    }
}
