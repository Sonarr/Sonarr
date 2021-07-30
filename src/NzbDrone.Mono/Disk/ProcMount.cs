using System.IO;
using Mono.Unix;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Mono.Disk
{
    public class ProcMount : IMount
    {
        private readonly UnixDriveInfo _unixDriveInfo;

        public ProcMount(DriveType driveType, string name, string mount, string type, MountOptions mountOptions)
        {
            DriveType = driveType;
            Name = name;
            RootDirectory = mount;
            DriveFormat = type;
            MountOptions = mountOptions;

            _unixDriveInfo = new UnixDriveInfo(mount);
        }

        public long AvailableFreeSpace => _unixDriveInfo.AvailableFreeSpace;

        public string DriveFormat { get; private set; }

        public DriveType DriveType { get; private set; }

        public bool IsReady => _unixDriveInfo.IsReady;

        public MountOptions MountOptions { get; private set; }

        public string Name { get; private set; }

        public string RootDirectory { get; private set; }

        public long TotalFreeSpace => _unixDriveInfo.TotalFreeSpace;

        public long TotalSize => _unixDriveInfo.TotalSize;

        public string VolumeLabel => _unixDriveInfo.VolumeLabel;

        public string VolumeName
        {
            get
            {
                if (VolumeLabel.IsNullOrWhiteSpace() || VolumeLabel.StartsWith("UUID=") || Name == VolumeLabel)
                {
                    return Name;
                }

                return string.Format("{0} ({1})", Name, VolumeLabel);
            }
        }
    }
}
