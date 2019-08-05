using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Download
{
    public class DownloadIgnoredEvent : IEvent
    {
        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public Language Language { get; set; }
        public QualityModel Quality { get; set; }
        public string SourceTitle { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }
        public string Message { get; set; }
    }
}