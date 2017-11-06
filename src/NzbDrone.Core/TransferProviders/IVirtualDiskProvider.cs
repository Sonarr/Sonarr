using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.TransferProviders
{
    // Represents the remote filesystem, or contents of rar, or ... etc.
    // Any Move/Copy action should return an asynchroneous context representing the transfer in progress. So it can be shown in CDH / Activity->Queue.
    public interface  IVirtualDiskProvider // : IDiskProvider
    {
        // Copies file from the virtual filesystem to the actual one.
        TransferTask CopyFile(string vfsSourcePath, string destinationPath);
    }
}
