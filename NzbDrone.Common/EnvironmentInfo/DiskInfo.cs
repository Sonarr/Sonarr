using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NzbDrone.Common.Model;

namespace NzbDrone.Common.EnvironmentInfo
{
    public static class DiskInfo
    {
        public static DiskSpace[] AllDrivesFreeSpace()
        {
            return (DriveInfo.GetDrives()
                .Where(driveInfo => driveInfo.DriveType == DriveType.Fixed)
                .Select(
                    driveInfo =>
                        new DiskSpace() {DriveLetter = driveInfo.Name, FreeSpace = SizeSuffix(driveInfo.TotalFreeSpace)})).ToArray();
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
