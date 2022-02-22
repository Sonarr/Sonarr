using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class GrabMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }
        public RemoteEpisode Episode { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadClientName { get; set; }
        public string DownloadId { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
