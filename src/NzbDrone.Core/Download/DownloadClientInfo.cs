using System.Collections.Generic;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Download
{
    public class DownloadClientInfo
    {
        public DownloadClientInfo()
        {
            OutputRootFolders = new List<OsPath>();
        }

        public bool IsLocalhost { get; set; }
        public string SortingMode { get; set; }
        public bool RemovesCompletedDownloads { get; set; }
        public List<OsPath> OutputRootFolders { get; set; }
    }
}
