using Workarr.Download.TrackedDownloads;
using Workarr.Languages;
using Workarr.Messaging;
using Workarr.Parser.Model;
using Workarr.Qualities;

namespace Workarr.Download
{
    public class DownloadFailedEvent : IEvent
    {
        public DownloadFailedEvent()
        {
            Data = new Dictionary<string, string>();
        }

        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public QualityModel Quality { get; set; }
        public string SourceTitle { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public List<Language> Languages { get; set; }
        public bool SkipRedownload { get; set; }
        public ReleaseSourceType ReleaseSource { get; set; }
    }
}
