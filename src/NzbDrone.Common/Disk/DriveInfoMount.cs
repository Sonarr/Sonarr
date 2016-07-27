using System.IO;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Disk
{
    public class DriveInfoMount : IMount
    {
        private readonly DriveInfo _driveInfo;
        private readonly DriveType _driveType;

        public DriveInfoMount(DriveInfo driveInfo, DriveType driveType = DriveType.Unknown)
        {
            _driveInfo = driveInfo;
            _driveType = driveType;
        }

        public long AvailableFreeSpace
        {
            get { return _driveInfo.AvailableFreeSpace; }
        }

        public string DriveFormat
        {
            get { return _driveInfo.DriveFormat; }
        }

        public DriveType DriveType
        {
            get
            {
                if (_driveType != DriveType.Unknown)
                {
                    return _driveType;
                }

                return _driveInfo.DriveType;
            }
        }

        public bool IsReady
        {
            get { return _driveInfo.IsReady; }
        }

        public string Name
        {
            get { return _driveInfo.Name; }
        }

        public string RootDirectory
        {
            get { return _driveInfo.RootDirectory.FullName; }
        }

        public long TotalFreeSpace
        {
            get { return _driveInfo.TotalFreeSpace; }
        }

        public long TotalSize
        {
            get { return _driveInfo.TotalSize; }
        }

        public string VolumeLabel
        {
            get { return _driveInfo.VolumeLabel; }
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
