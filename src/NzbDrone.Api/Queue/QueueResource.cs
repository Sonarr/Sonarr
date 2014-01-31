using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Queue
{
    public class QueueResource : RestResource
    {
        public Core.Tv.Series Series { get; set; }
        public Episode Episode { get; set; }
        public QualityModel Quality { get; set; }
        public Decimal Size { get; set; }
        public String Title { get; set; }
        public Decimal Sizeleft { get; set; }
        public TimeSpan Timeleft { get; set; }
        public String Status { get; set; }
    }
}
