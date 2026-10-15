using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download
{
    public class DownloadCompletedEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }
        public Series Series { get; private set; }
        public List<Episode> Episodes { get; private set; }
        public List<EpisodeFile> EpisodeFiles { get; private set; }
        public GrabbedReleaseInfo Release { get; private set; }

        public DownloadCompletedEvent(TrackedDownload trackedDownload, Series series, List<Episode> episodes, List<EpisodeFile> episodeFiles, GrabbedReleaseInfo release)
        {
            TrackedDownload = trackedDownload;
            Series = series;
            Episodes = episodes;
            EpisodeFiles = episodeFiles;
            Release = release;
        }
    }
}
