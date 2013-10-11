using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NzbDrone.Common;

namespace NzbDrone.Api.DiskSpace
{
    public class DiskSpaceModule :NzbDroneRestModule<DiskSpaceResource>
    {
        private readonly IDiskProvider _diskProvider;

        public DiskSpaceModule(IDiskProvider diskProvider):base("diskspace")
        {
            _diskProvider = diskProvider;
            GetResourceAll = GetFreeSpace;
        }

        public List<DiskSpaceResource> GetFreeSpace()
        {
            return (_diskProvider.GetFixedDrives()
                .Select(
                    x =>
                        new DiskSpaceResource()
                        {
                            DriveLetter = x,
                            FreeSpace = _diskProvider.GetAvailableSpace(x).Value,
                            TotalSpace = _diskProvider.GetTotalSize(x).Value
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
