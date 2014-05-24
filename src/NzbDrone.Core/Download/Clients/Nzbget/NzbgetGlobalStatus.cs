using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetGlobalStatus
    {
        public UInt32 RemainingSizeLo { get; set; }
        public UInt32 RemainingSizeHi { get; set; }
        public UInt32 DownloadedSizeLo { get; set; }
        public UInt32 DownloadedSizeHi { get; set; }
        public UInt32 DownloadRate { get; set; }
        public UInt32 AverageDownloadRate { get; set; }
        public UInt32 DownloadLimit { get; set; }
        public Boolean DownloadPaused { get; set; }
    }
}
