using Workarr.Download.TrackedDownloads;
using Workarr.MediaFiles;
using Workarr.Messaging;
using Workarr.Parser.Model;

namespace Workarr.Download
{
    public class DownloadCompletedEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }
        public int SeriesId { get; private set; }
        public List<EpisodeFile> EpisodeFiles { get; private set; }
        public GrabbedReleaseInfo Release { get; private set; }

        public DownloadCompletedEvent(TrackedDownload trackedDownload, int seriesId, List<EpisodeFile> episodeFiles, GrabbedReleaseInfo release)
        {
            TrackedDownload = trackedDownload;
            SeriesId = seriesId;
            EpisodeFiles = episodeFiles;
            Release = release;
        }
    }
}
