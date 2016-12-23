namespace NzbDrone.Core.DiskSpace
{
    public class DiskSpace
    {
        public string Path { get; set; }
        public string Label { get; set; }
        public long FreeSpace { get; set; }
        public long TotalSpace { get; set; }
    }
}
