using Workarr.Download;
using Workarr.Messaging;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.Events
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
