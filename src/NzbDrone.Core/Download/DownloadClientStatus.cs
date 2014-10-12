using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Download
{
    public class DownloadClientStatus
    {
        public Boolean IsLocalhost { get; set; }
        public List<OsPath> OutputRootFolders { get; set; }
    }
}
