using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model.TvRage
{
    public class TvRageSearchResult
    {
        public int ShowId { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Country { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Ended { get; set; }
        public int Seasons { get; set; }
        public string Status { get; set; }
        public int RunTime { get; set; }
        public DateTime AirTime { get; set; }
        public DayOfWeek AirDay { get; set; }
    }
}
