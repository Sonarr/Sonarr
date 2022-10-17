using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public class ManualInteractionEvent : IEvent
    {
        public RemoteEpisode Episode { get; private set; }
        public TrackedDownload TrackedDownload { get; private set; }
        public int DownloadClientId { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientName { get; set; }
        public string DownloadId { get; set; }

        public ManualInteractionEvent(TrackedDownload trackedDownload)
        {
            TrackedDownload = trackedDownload;
            Episode = trackedDownload.RemoteEpisode;
        }
    }
}
