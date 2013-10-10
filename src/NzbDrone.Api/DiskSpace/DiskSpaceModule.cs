using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NzbDrone.Api.DiskSpace
{
    public class DiskSpaceModule :NzbDroneRestModule<DiskSpaceResource>
    {
        public DiskSpaceModule():base("diskspace")
        {
            GetResourceAll = GetFreeSpace;
        }
        public List<DiskSpaceResource> GetFreeSpace()
        {
            return (DriveInfo.GetDrives()
                .Where(driveInfo => driveInfo.DriveType == DriveType.Fixed)
                .Select(
                    driveInfo =>
                        new DiskSpaceResource()
                        {
                            DriveLetter = driveInfo.Name,
                            FreeSpace = SizeSuffix(driveInfo.TotalFreeSpace),
                            TotalSpace = SizeSuffix(driveInfo.TotalSize)
                        })).ToList();
        }

        static string SizeSuffix(Int64 value)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue / 1024) >= 1)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n1}{1}", dValue, suffixes[i]);
        }
    }
}
