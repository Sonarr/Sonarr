using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.TransferProviders
{
    // Represents the remote filesystem, or contents of rar, or ... etc.
    // Any Move/Copy action should return an asynchroneous context representing the transfer in progress. So it can be shown in CDH / Activity->Queue.
    public interface IVirtualDiskProvider // : IDiskProvider
    {
        // Whether the VirtualFileSystem supports direct streaming of the file content.
        bool SupportStreaming { get; }

        // Returns recursive list of all files in the 'volume'/'filesystem'/'dataset' (whatever we want to call it).
        string[] GetFiles();

        // Opens a readable stream.
        Stream OpenFile(string vfsFilePath);

        // Copies file from the virtual filesystem to the actual one.
        TransferTask CopyFile(string vfsSourcePath, string destinationPath);

        // Move file from the virtual filesystem to the actual one.
        TransferTask MoveFile(string vfsSourcePath, string destinationPath);
    }
}
