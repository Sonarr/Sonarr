using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeImportFailedEvent : IEvent
    {
        public Exception Exception { get; set; }
        public LocalEpisode EpisodeInfo { get; }
        public bool NewDownload { get; }
        public DownloadClientItemClientInfo DownloadClientInfo { get;  }
        public string DownloadId { get; }

        public EpisodeImportFailedEvent(Exception exception, LocalEpisode episodeInfo, bool newDownload, DownloadClientItem downloadClientItem)
        {
            Exception = exception;
            EpisodeInfo = episodeInfo;
            NewDownload = newDownload;

            if (downloadClientItem != null)
            {
                DownloadClientInfo = downloadClientItem.DownloadClientInfo;
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
