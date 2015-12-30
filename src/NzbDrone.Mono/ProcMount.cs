using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using Mono.Unix;

namespace NzbDrone.Mono
{
    public class ProcMount : IMount
    {
        private readonly UnixDriveInfo _unixDriveInfo;

        public ProcMount(DriveType driveType, string name, string mount, string type, Dictionary<string, string> options)
        {
            DriveType = driveType;
            Name = name;
            RootDirectory = mount;
            DriveFormat = type;

            _unixDriveInfo = new UnixDriveInfo(mount);
        }

        public long AvailableFreeSpace
        {
            get { return _unixDriveInfo.AvailableFreeSpace; }
        }

        public string DriveFormat { get; private set; }

        public DriveType DriveType { get; private set; }

        public bool IsReady
        {
            get { return _unixDriveInfo.IsReady; }
        }

        public string Name { get; private set; }

        public string RootDirectory { get; private set; }

        public long TotalFreeSpace
        {
            get { return _unixDriveInfo.TotalFreeSpace; }
        }

        public long TotalSize
        {
            get { return _unixDriveInfo.TotalSize; }
        }

        public string VolumeLabel
        {
            get { return _unixDriveInfo.VolumeLabel; }
        }
    }
}
