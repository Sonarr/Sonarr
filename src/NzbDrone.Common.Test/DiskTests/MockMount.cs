using System.IO;
using NzbDrone.Common.Disk;

namespace NzbDrone.Common.Test.DiskTests
{
    public class MockMount : IMount
    {
        public long AvailableFreeSpace { get; set; }

        public string DriveFormat { get; set; }

        public DriveType DriveType { get; set; } = DriveType.Fixed;

        public bool IsReady { get; set; } = true;

        public MountOptions MountOptions { get; set; }

        public string Name { get; set; }

        public string RootDirectory { get; set; }

        public long TotalFreeSpace { get; set; }

        public long TotalSize { get; set; }

        public string VolumeLabel { get; set; }

        public string VolumeName { get; set; }
    }
}
