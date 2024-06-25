using System.Collections.Generic;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class ImportCompleteMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public List<EpisodeFile> EpisodeFiles { get; set; }
        public string SourcePath { get; set; }
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
