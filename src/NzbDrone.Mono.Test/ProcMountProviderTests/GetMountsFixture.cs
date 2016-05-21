using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;

namespace NzbDrone.Mono.Test.ProcMountProviderTests
{
    [TestFixture]
    public class GetMountsFixture : TestBase<ProcMountProvider>
    {
        [SetUp]
        public void Setup()
        {
            MonoOnly();
        }

        [Test]
        public void should_parse_proc_mounts()
        {
            const string procMountFile = @"/proc/mounts";

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(procMountFile))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.ReadAllLines(procMountFile))
                  .Returns(new []
                           {
                               "/dev/sda1 /boot ext2 rw,relatime 0 0"
                           });

            var result = Subject.GetMounts().First();
            result.Name.Should().Be("/dev/sda1");
            result.RootDirectory.Should().Be("/boot");
            result.DriveType.Should().Be(DriveType.Fixed);
        }

        [Test]
        public void should_remove_octal_escape_codes_from_mounts()
        {
            const string procMountFile = @"/proc/mounts";

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(procMountFile))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.ReadAllLines(procMountFile))
                  .Returns(new[]
                           {
                               "/dev/sda1 /boot\\040drive ext2 rw,relatime 0 0"
                           });

            var result = Subject.GetMounts();         
            result.Should().HaveCount(1);
            result.First().RootDirectory.Should().Be("/boot drive");
        }
    }
}
