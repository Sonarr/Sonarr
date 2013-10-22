using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download
{
    public class DownloadFailedEvent : IEvent
    {
        public Series Series { get; set; }
        public Episode Episode { get; set; }
        public QualityModel Quality { get; set; }
        public String SourceTitle { get; set; }
        public String DownloadClient { get; set; }
        public String DownloadClientId { get; set; }
    }
}