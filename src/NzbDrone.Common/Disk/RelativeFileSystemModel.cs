using System;

namespace NzbDrone.Common.Disk
{
    public class RelativeFileSystemModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
