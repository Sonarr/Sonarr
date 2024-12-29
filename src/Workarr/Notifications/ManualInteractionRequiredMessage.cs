using Workarr.Download;
using Workarr.Download.TrackedDownloads;
using Workarr.Parser.Model;
using Workarr.Qualities;
using Workarr.Tv;

namespace Workarr.Notifications
{
    public class ManualInteractionRequiredMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }
        public RemoteEpisode Episode { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public QualityModel Quality { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public GrabbedReleaseInfo Release { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
