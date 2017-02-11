using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskTests
{
    [TestFixture]
    public class DirectoryLookupServiceFixture : TestBase<FileSystemLookupService>
    {
        private const string RECYCLING_BIN = "$Recycle.Bin";
        private const string SYSTEM_VOLUME_INFORMATION = "System Volume Information";
        private const string WINDOWS = "Windows";
        private List<DirectoryInfo> _folders;

        private void SetupFolders(string root)
        {
            var folders = new List<string>
            {
                RECYCLING_BIN,
                "Chocolatey",
                "Documents and Settings",
                "Dropbox",
                "Intel",
                "PerfLogs",
                "Program Files",
                "Program Files (x86)",
                "ProgramData",
                SYSTEM_VOLUME_INFORMATION,
                "Test",
                "Users",
                WINDOWS
            };

            _folders = folders.Select(f => new DirectoryInfo(Path.Combine(root, f))).ToList();
        }

        [Test]
        public void should_not_contain_recycling_bin_for_root_of_drive()
        {
            string root = @"C:\".AsOsAgnostic();
            SetupFolders(root);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectoryInfos(It.IsAny<string>()))
                .Returns(_folders);

            Subject.LookupContents(root, false, false).Directories.Should().NotContain(Path.Combine(root, RECYCLING_BIN));
        }

        [Test]
        public void should_not_contain_system_volume_information()
        {
            string root = @"C:\".AsOsAgnostic();
            SetupFolders(root);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectoryInfos(It.IsAny<string>()))
                .Returns(_folders);

            Subject.LookupContents(root, false, false).Directories.Should().NotContain(Path.Combine(root, SYSTEM_VOLUME_INFORMATION));
        }

        [Test]
        public void should_not_contain_recycling_bin_or_system_volume_information_for_root_of_drive()
        {
            string root = @"C:\".AsOsAgnostic();
            SetupFolders(root);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectoryInfos(It.IsAny<string>()))
                .Returns(_folders);

            var result = Subject.LookupContents(root, false, false);
            
            result.Directories.Should().HaveCount(_folders.Count - 3);

            result.Directories.Should().NotContain(f => f.Name == RECYCLING_BIN);
            result.Directories.Should().NotContain(f => f.Name == SYSTEM_VOLUME_INFORMATION);
            result.Directories.Should().NotContain(f => f.Name == WINDOWS);
        }
    }
}
