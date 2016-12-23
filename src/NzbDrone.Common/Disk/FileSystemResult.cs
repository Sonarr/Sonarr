using System.Collections.Generic;

namespace NzbDrone.Common.Disk
{
    public class FileSystemResult
    {
        public string Parent { get; set; }
        public List<FileSystemModel> Directories { get; set; }
        public List<FileSystemModel> Files { get; set; }

        public FileSystemResult()
        {
            Directories = new List<FileSystemModel>();
            Files = new List<FileSystemModel>();
        }
    }
}
