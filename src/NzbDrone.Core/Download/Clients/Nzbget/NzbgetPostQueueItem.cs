using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetPostQueueItem
    {
        public Int32 NzbId { get; set; }
        public String NzbName { get; set; }
        public String Stage { get; set; }
        public String ProgressLabel { get; set; }
        public Int32 FileProgress { get; set; }
        public Int32 StageProgress { get; set; }
        public Int32 TotalTimeSec { get; set; }
        public Int32 StageTimeSec { get; set; }
    }
}
