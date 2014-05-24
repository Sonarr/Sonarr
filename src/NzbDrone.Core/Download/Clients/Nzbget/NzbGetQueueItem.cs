using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetQueueItem
    {
        public Int32 NzbId { get; set; }
        public Int32 FirstId { get; set; }
        public Int32 LastId { get; set; }
        public String NzbName { get; set; }
        public String Category { get; set; }
        public UInt32 FileSizeLo { get; set; }
        public UInt32 FileSizeHi { get; set; }
        public UInt32 RemainingSizeLo { get; set; }
        public UInt32 RemainingSizeHi { get; set; }
        public UInt32 PausedSizeLo { get; set; }
        public UInt32 PausedSizeHi { get; set; }
        public Int32 MinPriority { get; set; }
        public Int32 MaxPriority { get; set; }
        public Int32 ActiveDownloads { get; set; }
        public List<NzbgetParameter> Parameters { get; set; }
    }
}
