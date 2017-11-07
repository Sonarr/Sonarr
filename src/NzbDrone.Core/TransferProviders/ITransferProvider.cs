using System;
using System.Linq;
using NzbDrone.Core.Download;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.TransferProviders
{
    public interface ITransferProvider : IProvider
    {
        // TODO: Perhaps change 'string' to 'DownloadClientPath' struct/class so we're more typesafe.

        // Whether the TransferProvider is ready to be accessed. (Useful for external transfers that may not have finished yet)
        bool IsAvailable(string downloadClientPath);
        bool IsAvailable(DownloadClientItem item);

        // Returns a wrapper for the specific download. Optionally we might want to supply a 'tempDir' that's close to the series path, in case the TransferProvider needs an intermediate location.
        IVirtualDiskProvider GetFileSystemWrapper(string downloadClientPath);
        IVirtualDiskProvider GetFileSystemWrapper(DownloadClientItem item);
    }
}
