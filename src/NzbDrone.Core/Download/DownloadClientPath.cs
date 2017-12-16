using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Download
{
    public class DownloadClientPath
    {
        public int DownloadClientId { get; set; }
        public OsPath Path { get; set; }

        public DownloadClientPath(int downloadClientId, OsPath path)
        {
            DownloadClientId = downloadClientId;
            Path = path;
        }
    }
}
