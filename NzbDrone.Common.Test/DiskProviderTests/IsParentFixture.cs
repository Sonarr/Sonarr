using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskProviderTests
{
    [TestFixture]
    public class FreeSpaceFixture : TestBase<DiskProvider>
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
        public void should_return_null_when_cant_get_free_space()
        {
            LinuxOnly();

           Subject.GetAvailableSpace("/run/").Should().NotBe(null);
        }
    }
}
