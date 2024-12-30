using Workarr.Download.TrackedDownloads;
using Workarr.Messaging;
using Workarr.Parser.Model;

namespace Workarr.Download
{
    public class ManualInteractionRequiredEvent : IEvent
    {
        public RemoteEpisode Episode { get; private set; }
        public TrackedDownload TrackedDownload { get; private set; }
        public GrabbedReleaseInfo Release { get; private set; }

        public ManualInteractionRequiredEvent(TrackedDownload trackedDownload, GrabbedReleaseInfo release)
        {
            TrackedDownload = trackedDownload;
            Episode = trackedDownload.RemoteEpisode;
            Release = release;
        }
    }
}
