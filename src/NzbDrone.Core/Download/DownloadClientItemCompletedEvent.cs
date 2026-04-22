using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;

namespace NzbDrone.Core.Download
{
    public class DownloadClientItemCompletedEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }

        public DownloadClientItemCompletedEvent(TrackedDownload trackedDownload)
        {
            TrackedDownload = trackedDownload;
        }
    }
}
