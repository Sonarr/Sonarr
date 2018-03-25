using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using System;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeDownloadFailedEvent : IEvent
    {

        public Exception Exception { get; set; }
        public LocalEpisode EpisodeInfo { get; }
        public EpisodeFile FailedEpisode { get; private set; }
        public bool NewDownload { get; private set; }
        public string DownloadClient { get; private set; }
        public string DownloadId { get; private set; }

        public EpisodeDownloadFailedEvent(Exception exception, LocalEpisode episodeInfo, EpisodeFile failedEpisode, bool newDownload, DownloadClientItem downloadClientItem)
        {
            Exception = exception;
            EpisodeInfo = episodeInfo;
            FailedEpisode = failedEpisode;
            NewDownload = newDownload;

            if (downloadClientItem != null)
            {
                DownloadClient = downloadClientItem.DownloadClient;
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
