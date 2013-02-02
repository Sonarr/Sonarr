using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;
using System.IO;

namespace NzbDrone.Core.Test.ProviderTests.DiskProviderTests
{
    [TestFixture]
    public class ExtractArchiveFixture : SqlCeTest
    {
        [Test]
        public void Should_extract_to_correct_folder()
        {
            var destination = Path.Combine(TempFolder, "destination");
            Mocker.Resolve<ArchiveProvider>().ExtractArchive(GetTestFilePath("TestArchive.zip"), destination);

            var destinationFolder = new DirectoryInfo(destination);

            destinationFolder.Exists.Should().BeTrue();
            destinationFolder.GetDirectories().Should().HaveCount(1);
            destinationFolder.GetDirectories("*", SearchOption.AllDirectories).Should().HaveCount(3);
            destinationFolder.GetFiles("*.*", SearchOption.AllDirectories).Should().HaveCount(6);
        }
    }
}
