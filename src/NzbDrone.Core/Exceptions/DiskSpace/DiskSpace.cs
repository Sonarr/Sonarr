using System;

namespace NzbDrone.Core.DiskSpace
{
    public class DiskSpace
    {
        public String Path { get; set; }
        public String Label { get; set; }
        public long FreeSpace { get; set; }
        public long TotalSpace { get; set; }
    }
}
