using System.IO;
using NzbDrone.Common.Disk;

namespace NzbDrone.Windows.Disk
{
    public class FolderMount : IMount
    {
        private readonly DirectoryInfo _directoryInfo;

        public FolderMount(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        public long AvailableFreeSpace => 0;

        public string DriveFormat => "NTFS";

        public DriveType DriveType => DriveType.Removable;

        public bool IsReady => true;

        public MountOptions MountOptions { get; private set; }

        public string Name => _directoryInfo.Name;

        public string RootDirectory => _directoryInfo.FullName;

        public long TotalFreeSpace => 0;

        public long TotalSize => 0;

        public string VolumeLabel => _directoryInfo.Name;

        public string VolumeName => Name;
    }
}
