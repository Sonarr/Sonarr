using System.Collections.Generic;
using System.IO;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Mono.Disk
{
    public static class FindDriveType
    {
        private static readonly Dictionary<string, DriveType> DriveTypeMap = new ()
        {
            { "afpfs", DriveType.Network },
            { "apfs", DriveType.Fixed },
            { "fuse.mergerfs", DriveType.Fixed },
            { "fuse.shfs", DriveType.Fixed },
            { "fuse.glusterfs", DriveType.Network },
            { "nullfs", DriveType.Fixed },
            { "zfs", DriveType.Fixed }
        };

        public static DriveType Find(string driveFormat)
        {
            if (driveFormat.IsNullOrWhiteSpace())
            {
                return DriveType.Unknown;
            }

            return DriveTypeMap.GetValueOrDefault(driveFormat);
        }
    }
}
