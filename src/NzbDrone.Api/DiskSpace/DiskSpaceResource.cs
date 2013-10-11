using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.DiskSpace
{
    public class DiskSpaceResource : RestResource
    {
        public string DriveLetter { get; set; }
        public Int64 FreeSpace { get; set; }
        public Int64 TotalSpace { get; set; }
    }
}
