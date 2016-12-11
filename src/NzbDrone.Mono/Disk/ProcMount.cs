using System.Collections.Generic;
using System.IO;
using Mono.Unix;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Mono.Disk
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

        public string VolumeName
        {
            get
            {
                if (VolumeLabel.IsNullOrWhiteSpace())
                {
                    return Name;
                }

                return string.Format("{0} ({1})", Name, VolumeLabel);
            }
        }
    }
}
