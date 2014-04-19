using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeImportedEvent : IEvent
    {
        public LocalEpisode EpisodeInfo { get; private set; }
        public EpisodeFile ImportedEpisode { get; private set; }
        public Boolean NewDownload { get; set; }
        public String DownloadClient { get; set; }
        public String DownloadClientId { get; set; }

        public EpisodeImportedEvent(LocalEpisode episodeInfo, EpisodeFile importedEpisode, bool newDownload)
        {
            EpisodeInfo = episodeInfo;
            ImportedEpisode = importedEpisode;
            NewDownload = newDownload;
        }

        public EpisodeImportedEvent(LocalEpisode episodeInfo, EpisodeFile importedEpisode, bool newDownload, string downloadClient, string downloadClientId)
        {
            EpisodeInfo = episodeInfo;
            ImportedEpisode = importedEpisode;
            NewDownload = newDownload;
            DownloadClient = downloadClient;
            DownloadClientId = downloadClientId;
        }
    }
}