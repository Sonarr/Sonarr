using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetQueueItem
    {
        public int NzbId { get; set; }
        public int FirstId { get; set; }
        public int LastId { get; set; }
        public string NzbName { get; set; }
        public string Category { get; set; }
        public uint FileSizeLo { get; set; }
        public uint FileSizeHi { get; set; }
        public uint RemainingSizeLo { get; set; }
        public uint RemainingSizeHi { get; set; }
        public uint PausedSizeLo { get; set; }
        public uint PausedSizeHi { get; set; }
        public int MinPriority { get; set; }
        public int MaxPriority { get; set; }
        public int ActiveDownloads { get; set; }
        public List<NzbgetParameter> Parameters { get; set; }
    }
}
