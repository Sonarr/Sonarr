using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Mono.EnvironmentInfo.VersionAdapters;
using NzbDrone.Test.Common;

namespace NzbDrone.Mono.Test.EnvironmentInfo.VersionAdapters
{
    [TestFixture]
    public class MacOsVersionAdapterFixture : TestBase<MacOsVersionAdapter>
    {
        [TestCase("10.8.0")]
        [TestCase("10.8")]
        [TestCase("10.8.1")]
        [TestCase("10.11.20")]
        public void should_get_version_info(string versionString)
        {
            var fileContent = File.ReadAllText(GetTestPath("Files/macOS/SystemVersion.plist")).Replace("10.0.0", versionString);

            const string plistPath = "/System/Library/CoreServices/SystemVersion.plist";

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FolderExists("/System/Library/CoreServices/")).Returns(true);

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetFiles("/System/Library/CoreServices/", SearchOption.TopDirectoryOnly))
                .Returns(new[] { plistPath });

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.ReadAllText(plistPath))
                .Returns(fileContent);

            var versionName = Subject.Read();
            versionName.Version.Should().Be(versionString);
            versionName.Name.Should().Be("macOS");
            versionName.FullName.Should().Be("macOS " + versionString);
        }


        [TestCase]
        public void should_detect_server()
        {
            var fileContent = File.ReadAllText(GetTestPath("Files/macOS/SystemVersion.plist"));

            const string plistPath = "/System/Library/CoreServices/ServerVersion.plist";

            Mocker.GetMock<IDiskProvider>()
               .Setup(c => c.FolderExists("/System/Library/CoreServices/")).Returns(true);

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetFiles("/System/Library/CoreServices/", SearchOption.TopDirectoryOnly))
                .Returns(new[] { plistPath });

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.ReadAllText(plistPath))
                .Returns(fileContent);

            var versionName = Subject.Read();
            versionName.Name.Should().Be("macOS Server");
        }

        [TestCase]
        public void should_return_null_if_folder_doesnt_exist()
        {
            Mocker.GetMock<IDiskProvider>()
               .Setup(c => c.FolderExists("/System/Library/CoreServices/")).Returns(false);

            Subject.Read().Should().BeNull();

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.GetFiles(It.IsAny<string>(), SearchOption.TopDirectoryOnly), Times.Never());
        }
    }
}
