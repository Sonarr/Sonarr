using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public enum HdrFormat
    {
        None,
        UnknownHdr,
        Pq10,
        Hdr10,
        Hdr10Plus,
        Hlg10,
        DolbyVision,
        DolbyVisionHdr10
    }
}
