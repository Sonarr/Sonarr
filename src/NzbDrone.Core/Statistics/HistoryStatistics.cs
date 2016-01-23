using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Statistics
{
    public class HistoryStatistics
    {
        public int Grabs { get; set; }
        public int Replaced { get; set; }
        public int Failed { get; set; }
        public int Imported { get; set; }
    }
}
