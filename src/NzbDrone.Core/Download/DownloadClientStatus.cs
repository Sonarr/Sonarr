using System.Collections.Generic;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Download
{
    public class DownloadClientStatus
    {
        public bool IsLocalhost { get; set; }
        public List<OsPath> OutputRootFolders { get; set; }
    }
}
