using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download
{
    public class DownloadClientStatus
    {
        public Boolean IsLocalhost { get; set; }
        public List<String> OutputRootFolders { get; set; }
    }
}
