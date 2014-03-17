using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskProviderTests
{
    public abstract class FreeSpaceFixtureBase<TSubject> : TestBase<TSubject> where TSubject : class, IDiskProvider
    {
        [Test]
        public void should_get_free_space_for_folder()
        {
            var path = @"C:\".AsOsAgnostic();

            Subject.GetAvailableSpace(path).Should().NotBe(0);
        }

        [Test]
        public void should_get_free_space_for_folder_that_doesnt_exist()
        {
            var path = @"C:\".AsOsAgnostic();

            Subject.GetAvailableSpace(Path.Combine(path, "invalidFolder")).Should().NotBe(0);
        }

        [Test]
        public void should_get_free_space_for_drive_that_doesnt_exist()
        {
            WindowsOnly();

            Assert.Throws<DirectoryNotFoundException>(() => Subject.GetAvailableSpace("J:\\").Should().NotBe(0));
        }

        [Test]
        public void should_be_able_to_check_space_on_ramdrive()
        {
            MonoOnly();
            Subject.GetAvailableSpace("/run/").Should().NotBe(0);
        }

        [Test]
        public void should_return_free_disk_space()
        {
            var result = Subject.GetAvailableSpace(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            result.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_able_to_get_space_on_unc()
        {
            WindowsOnly();

            var result = Subject.GetAvailableSpace(@"\\localhost\c$\Windows");
            result.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_throw_if_drive_doesnt_exist()
        {
            WindowsOnly();

            Assert.Throws<DirectoryNotFoundException>(() => Subject.GetAvailableSpace(@"Z:\NOT_A_REAL_PATH\DOES_NOT_EXIST".AsOsAgnostic()));
        }

        [Test]
        public void should_be_able_to_get_space_on_folder_that_doesnt_exist()
        {
            var result = Subject.GetAvailableSpace(@"C:\I_DO_NOT_EXIST".AsOsAgnostic());
            result.Should().BeGreaterThan(0);
        }
    }
}
