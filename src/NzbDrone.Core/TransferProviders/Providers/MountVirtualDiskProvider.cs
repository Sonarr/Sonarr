using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.TransferProviders.Providers
{
    // Empty wrapper, it would server in dealing with stuff being slower and remote mounts potentially being unavailable temporarily.
    // Ideally it should wrap a DirectVirtualDiskProvider instance, rather than inheriting from it.
    public class MountVirtualDiskProvider : DirectVirtualDiskProvider
    {
        public MountVirtualDiskProvider(IDiskProvider diskProvider, IDiskTransferService transferService, string rootFolder, params string[] items)
            : base(diskProvider, transferService, rootFolder, items)
        {

        }
    }
}
