using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Test.DiskTests;
using NzbDrone.Windows.Disk;

namespace NzbDrone.Windows.Test.DiskProviderTests
{
    [TestFixture]
    [Platform("Win")]
    public class FreeSpaceFixture : FreeSpaceFixtureBase<DiskProvider>
    {
        public FreeSpaceFixture()
        {
            WindowsOnly();
        }

        [Test]
        public void should_throw_if_drive_doesnt_exist()
        {
            // Find a drive that doesn't exist.
            for (char driveletter = 'Z'; driveletter > 'D'; driveletter--)
            {
                if (new DriveInfo(driveletter.ToString()).IsReady)
                {
                    continue;
                }

                Assert.Throws<DirectoryNotFoundException>(() => Subject.GetAvailableSpace(driveletter + @":\NOT_A_REAL_PATH\DOES_NOT_EXIST"));
                return;
            }

            Assert.Inconclusive("No drive available for testing.");
        }

        [Test]
        public void should_be_able_to_get_space_on_unc()
        {
            var result = Subject.GetAvailableSpace(@"\\localhost\c$\Windows");
            result.Should().BeGreaterThan(0);
        }
    }
}
