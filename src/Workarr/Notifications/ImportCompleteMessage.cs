using Workarr.Download;
using Workarr.MediaFiles;
using Workarr.Parser.Model;
using Workarr.Qualities;
using Workarr.Tv;

namespace Workarr.Notifications
{
    public class ImportCompleteMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public List<EpisodeFile> EpisodeFiles { get; set; }
        public string SourcePath { get; set; }
        public string SourceTitle { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public GrabbedReleaseInfo Release { get; set; }
        public string DestinationPath { get; set; }
        public string ReleaseGroup { get; set; }
        public QualityModel ReleaseQuality { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
