using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model.TvRage
{
    public class TvRageSeries
    {
        public int ShowId { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public int Seasons { get; set; }
        public int Started { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Ended { get; set; }
        public string OriginCountry { get; set; }
        public string Status { get; set; }
        public int RunTime { get; set; }
        public string Network { get; set; }
        public DateTime AirTime { get; set; }
        public DayOfWeek? AirDay { get; set; }
        public int UtcOffset { get; set; }
    }
}
