using Workarr.Messaging;

namespace Workarr.Download.TrackedDownloads
{
    public class TrackedDownloadsRemovedEvent : IEvent
    {
        public List<TrackedDownload> TrackedDownloads { get; private set; }

        public TrackedDownloadsRemovedEvent(List<TrackedDownload> trackedDownloads)
        {
            TrackedDownloads = trackedDownloads;
        }
    }
}
