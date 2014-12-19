using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public class TrackedDownloadRefreshedEvent : IEvent
    {
        public List<TrackedDownload> TrackedDownloads { get; private set; }

        public TrackedDownloadRefreshedEvent(List<TrackedDownload> trackedDownloads)
        {
            TrackedDownloads = trackedDownloads;
        }
    }
}
