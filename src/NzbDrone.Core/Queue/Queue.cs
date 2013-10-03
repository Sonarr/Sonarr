using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Queue
{
    public class Queue : ModelBase
    {
        public Series Series { get; set; }
        public Episode Episode { get; set; }
        public QualityModel Quality { get; set; }
        public Decimal Size { get; set; }
        public String Title { get; set; }
        public Decimal Sizeleft { get; set; }
        public TimeSpan Timeleft { get; set; }
        public String Status { get; set; }
    }
}
