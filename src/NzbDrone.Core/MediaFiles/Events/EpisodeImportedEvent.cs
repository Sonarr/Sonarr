using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles.Series;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeImportedEvent : IEvent
    {
        public LocalEpisode EpisodeInfo { get; private set; }
        public EpisodeFile ImportedEpisode { get; private set; }
        public Boolean NewDownload { get; private set; }
        public String DownloadClient { get; private set; }
        public String DownloadId { get; private set; }

        public EpisodeImportedEvent(LocalEpisode episodeInfo, EpisodeFile importedEpisode, bool newDownload)
        {
            EpisodeInfo = episodeInfo;
            ImportedEpisode = importedEpisode;
            NewDownload = newDownload;
        }

        public EpisodeImportedEvent(LocalEpisode episodeInfo, EpisodeFile importedEpisode, bool newDownload, string downloadClient, string downloadId)
        {
            EpisodeInfo = episodeInfo;
            ImportedEpisode = importedEpisode;
            NewDownload = newDownload;
            DownloadClient = downloadClient;
            DownloadId = downloadId;
        }
    }
}