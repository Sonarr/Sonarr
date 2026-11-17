using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Mono.Disk;
using NzbDrone.Test.Common;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    public class FindDriveTypeFixture : TestBase
    {
        [TestCase("virtiofs", DriveType.Fixed)]
        [TestCase("zfs", DriveType.Fixed)]
        [TestCase("afpfs", DriveType.Network)]
        [TestCase("unknownfs", DriveType.Unknown)]
        [TestCase("", DriveType.Unknown)]
        public void should_return_drive_type_for_drive_format(string driveFormat, DriveType driveType)
        {
            FindDriveType.Find(driveFormat).Should().Be(driveType);
        }
    }
}
