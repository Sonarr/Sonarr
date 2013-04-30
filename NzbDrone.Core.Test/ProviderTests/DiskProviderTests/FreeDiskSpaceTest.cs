using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DiskProviderTests
{
    [TestFixture]
    public class FreeDiskSpaceTest : CoreTest<DiskProvider>
    {
        [Test]
        public void should_return_free_disk_space()
        {
            var result = Subject.GetAvilableSpace(Directory.GetCurrentDirectory());
            result.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_able_to_get_space_on_unc()
        {
            WindowsOnly();

            var result = Subject.GetAvilableSpace(@"\\localhost\c$\Windows");
            result.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_throw_if_drive_doesnt_exist()
        {
            Assert.Throws<DirectoryNotFoundException>(() => Subject.GetAvilableSpace(@"Z:\NOT_A_REAL_PATH\DOES_NOT_EXIST"));
        }
    }
}
