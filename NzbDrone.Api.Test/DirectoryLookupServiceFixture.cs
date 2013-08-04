using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Api.Directories;
using NzbDrone.Common;
using NzbDrone.Test.Common;

namespace NzbDrone.Api.Test
{
    [TestFixture]
    public class DirectoryLookupServiceFixture :TestBase<DirectoryLookupService>
    {
        private const string RECYCLING_BIN = "$Recycle.Bin";
        private const string SYSTEM_VOLUME_INFORMATION = "System Volume Information";
        private List<String> _folders;

        [SetUp]
        public void Setup()
        {
            _folders = new List<String>
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
                "Windows"
            };

            Mocker.GetMock<IDiskProvider>()
                .SetupGet(s => s.SpecialFolders)
                .Returns(new HashSet<string> { "$recycle.bin", "system volume information", "recycler" });
        }

        private void SetupFolders(string root)
        {
            _folders.ForEach(e =>
            {
                e = Path.Combine(root, e);
            });
        }
            
        [Test]
        public void should_not_contain_recycling_bin_for_root_of_drive()
        {
            const string root = @"C:\";
            SetupFolders(root);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(It.IsAny<String>()))
                .Returns(_folders.ToArray());

            Subject.LookupSubDirectories(root).Should().NotContain(Path.Combine(root, RECYCLING_BIN));
        }

        [Test]
        public void should_not_contain_system_volume_information_for_root_of_drive()
        {
            const string root = @"C:\";
            SetupFolders(root);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(It.IsAny<String>()))
                .Returns(_folders.ToArray());

            Subject.LookupSubDirectories(root).Should().NotContain(Path.Combine(root, SYSTEM_VOLUME_INFORMATION));
        }

        [Test]
        public void should_not_contain_recycling_bin_or_system_volume_information_for_root_of_drive()
        {
            const string root = @"C:\";
            SetupFolders(root);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(It.IsAny<String>()))
                .Returns(_folders.ToArray());

            var result = Subject.LookupSubDirectories(root);

            result.Should().HaveCount(_folders.Count - 2);
            result.Should().NotContain(RECYCLING_BIN);
            result.Should().NotContain(SYSTEM_VOLUME_INFORMATION);
        }
    }
}
