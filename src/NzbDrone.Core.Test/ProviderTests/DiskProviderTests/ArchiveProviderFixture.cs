using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using System.IO;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.DiskProviderTests
{
    [TestFixture]
    public class ArchiveProviderFixture : TestBase<ArchiveService>
    {
        [Test]
        public void Should_extract_to_correct_folder()
        {
            var destination = Path.Combine(TempFolder, "destination");
            var testArchive = OsInfo.IsWindows ? "TestArchive.zip" : "TestArchive.tar.gz";

            Subject.Extract(GetTestFilePath(testArchive), destination);

            var destinationFolder = new DirectoryInfo(destination);

            destinationFolder.Exists.Should().BeTrue();
            destinationFolder.GetDirectories().Should().HaveCount(1);
            destinationFolder.GetDirectories("*", SearchOption.AllDirectories).Should().HaveCount(3);
            destinationFolder.GetFiles("*.*", SearchOption.AllDirectories).Should().HaveCount(6);
        }
    }
}
