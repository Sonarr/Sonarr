using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeImportedEvent : IEvent
    {
        public LocalEpisode EpisodeInfo { get; private set; }
        public EpisodeFile ImportedEpisode { get; private set; }
        public bool NewDownload { get; private set; }
        public string DownloadClient { get; private set; }
        public string DownloadId { get; private set; }
        public bool IsReadOnly { get; set; }

        public EpisodeImportedEvent(LocalEpisode episodeInfo, EpisodeFile importedEpisode, bool newDownload)
        {
            EpisodeInfo = episodeInfo;
            ImportedEpisode = importedEpisode;
            NewDownload = newDownload;
        }

        public EpisodeImportedEvent(LocalEpisode episodeInfo, EpisodeFile importedEpisode, bool newDownload, string downloadClient, string downloadId, bool isReadOnly)
        {
            EpisodeInfo = episodeInfo;
            ImportedEpisode = importedEpisode;
            NewDownload = newDownload;
            DownloadClient = downloadClient;
            DownloadId = downloadId;
            IsReadOnly = isReadOnly;
        }
    }
}