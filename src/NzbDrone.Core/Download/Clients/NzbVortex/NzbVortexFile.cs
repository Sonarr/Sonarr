namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public class NzbVortexFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public NzbVortexStateType State { get; set; }
        public long DileSize { get; set; }
        public long DownloadedSize { get; set; }
        public long TotalDownloadedSize { get; set; }
        public bool ExtractPasswordRequired { get; set; }
        public string ExtractPassword { get; set; }
        public long PostDate { get; set; }
        public bool Crc32CheckFailed { get; set; }
    }
}
