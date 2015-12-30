using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Disk
{
    public interface IMount
    {
        long AvailableFreeSpace { get; }
        string DriveFormat { get; }
        DriveType DriveType { get; }
        bool IsReady { get; }
        string Name { get; }
        string RootDirectory { get; }
        long TotalFreeSpace { get; }
        long TotalSize { get; }
        string VolumeLabel { get; }
    }
}
