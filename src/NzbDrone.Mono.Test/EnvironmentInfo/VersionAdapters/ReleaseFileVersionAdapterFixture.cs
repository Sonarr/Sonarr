using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Mono.Disk;
using NzbDrone.Mono.EnvironmentInfo.VersionAdapters;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Mono.Test.EnvironmentInfo.VersionAdapters
{
    [TestFixture]
    public class ReleaseFileVersionAdapterFixture : TestBase<ReleaseFileVersionAdapter>
    {
        [Test]
        [IntegrationTest]
        [Platform("Linux")]
        public void should_get_version_info_from_actual_linux()
        {
            NotBsd();

            Mocker.SetConstant<IDiskProvider>(Mocker.Resolve<DiskProvider>());
            var info = Subject.Read();
            info.FullName.Should().NotBeNullOrWhiteSpace();
            info.Name.Should().NotBeNullOrWhiteSpace();
            info.Version.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void should_return_null_if_etc_doestn_exist()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists("/etc/")).Returns(false);
            Subject.Read().Should().BeNull();

            Mocker.GetMock<IDiskProvider>()
              .Verify(c => c.GetFiles(It.IsAny<string>(), SearchOption.TopDirectoryOnly), Times.Never());

            Subject.Read().Should().BeNull();
        }

        [Test]
        public void should_return_null_if_release_file_doestn_exist()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists("/etc/")).Returns(true);
            Subject.Read().Should().BeNull();

            Mocker.GetMock<IDiskProvider>()
              .Setup(c => c.GetFiles(It.IsAny<string>(), SearchOption.TopDirectoryOnly)).Returns(Array.Empty<string>());

            Subject.Read().Should().BeNull();
        }

        [Test]
        public void should_detect_version()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists("/etc/")).Returns(true);
            Subject.Read().Should().BeNull();

            Mocker.GetMock<IDiskProvider>()
              .Setup(c => c.GetFiles(It.IsAny<string>(), SearchOption.TopDirectoryOnly)).Returns(new[]
                {
                    "/etc/lsb-release",
                    "/etc/os-release"
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.ReadAllText("/etc/lsb-release"))
                .Returns(File.ReadAllText(GetTestPath("Files/linux/lsb-release")));

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.ReadAllText("/etc/os-release"))
                .Returns(File.ReadAllText(GetTestPath("Files/linux/os-release")));

            var version = Subject.Read();
            version.Should().NotBeNull();
            version.Name.Should().Be("ubuntu");
            version.Version.Should().Be("14.04");
            version.FullName.Should().Be("Ubuntu 14.04.5 LTS");
        }
    }
}
