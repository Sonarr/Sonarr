using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.DiskProviderTests
{
    [TestFixture]
    public class FreeDiskSpaceFixture : CoreTest<DiskProvider>
    {
        [Test]
        public void should_return_free_disk_space()
        {
            var result = Subject.GetAvilableSpace(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
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
            WindowsOnly();

            Assert.Throws<DirectoryNotFoundException>(() => Subject.GetAvilableSpace(@"Z:\NOT_A_REAL_PATH\DOES_NOT_EXIST".AsOsAgnostic()));
        }

        [Test]
        public void should_be_able_to_get_space_on_folder_that_doesnt_exist()
        {
            var result = Subject.GetAvilableSpace(@"C:\I_DO_NOT_EXIST".AsOsAgnostic());
            result.Should().BeGreaterThan(0);
        }
    }
}
