using System;
using System.Linq;
using NzbDrone.Core.Download;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.TransferProviders
{
    public interface ITransferProvider : IProvider, IVirtualDiskProvider
    {
        // Whether the TransferProvider is ready to be accessed. (Useful for external transfers that may not have finished yet)
        bool IsAvailable(string downloadClientPath);
        bool IsAvailable(DownloadClientItem item);
    }
}
