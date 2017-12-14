using System;
using System.Linq;
using NzbDrone.Core.Download;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.TransferProviders
{
    public interface ITransferProvider : IProvider
    {
        // Whether the TransferProvider is ready to be accessed. (Useful for external transfers that may not have finished yet)
        bool IsAvailable(DownloadClientPath item);

        // Returns a wrapper for the specific download. Optionally we might want to supply a 'tempPath' that's close to the series path, in case the TransferProvider needs an intermediate location.
        IVirtualDiskProvider GetFileSystemWrapper(DownloadClientPath item, string tempPath = null);
    }
}
