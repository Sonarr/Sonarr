using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;

namespace NzbDrone.Core.Download
{
    public class DownloadCanBeRemovedEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }

        public DownloadCanBeRemovedEvent(TrackedDownload trackedDownload)
        {
            TrackedDownload = trackedDownload;
        }
    }
}
