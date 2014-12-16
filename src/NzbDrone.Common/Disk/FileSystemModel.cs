using System;

namespace NzbDrone.Common.Disk
{
    public class FileSystemModel
    {
        public FileSystemEntityType Type { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public enum FileSystemEntityType
    {
        Parent,
        Drive,
        Folder,
        File
    }
}
