using System;
using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download
{
    public class DownloadFailedEvent : IEvent
    {
        public Int32 SeriesId { get; set; }
        public List<Int32> EpisodeIds { get; set; }
        public QualityModel Quality { get; set; }
        public String SourceTitle { get; set; }
        public String DownloadClient { get; set; }
        public String DownloadClientId { get; set; }
        public String Message { get; set; }
    }
}