using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class ManualInteractionRequiredMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }
        public RemoteEpisode Episode { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadClientName { get; set; }
        public string DownloadId { get; set; }
        public GrabbedReleaseInfo Release { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
