using System;

namespace NzbDrone.Common.Disk
{
    [Flags]
    public enum TransferMode
    {
        None = 0,

        Move = 1,
        Copy = 2,
        HardLink = 4,

        HardLinkOrCopy = Copy | HardLink
    }
}
