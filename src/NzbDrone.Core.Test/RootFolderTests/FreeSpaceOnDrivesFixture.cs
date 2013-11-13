

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.RootFolderTests
{
    [TestFixture]

    public class FreeSpaceOnDrivesFixture : CoreTest<RootFolderService>
    {
        [Test]
        public void should_return_one_drive_when_only_one_root_dir_exists()
        {
            Mocker.GetMock<IRootFolderRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<RootFolder> { new RootFolder { Id = 1, Path = @"C:\Test\TV" } });

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetPathRoot(@"C:\Test\TV"))
                  .Returns(@"C:\");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetAvailableSpace(@"C:\"))
                  .Returns(123456);

            var result = Subject.FreeSpaceOnDrives();

            result.Should().HaveCount(1);
        }

        [Test]
        public void should_return_one_drive_when_two_rootDirs_on_the_same_drive_exist()
        {
            Mocker.GetMock<IRootFolderRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<RootFolder> { new RootFolder { Id = 1, Path = @"C:\Test\TV" },
                                             new RootFolder { Id = 2, Path = @"C:\Test\TV2" }});

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetPathRoot(It.IsAny<String>()))
                  .Returns(@"C:\");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetAvailableSpace(@"C:\"))
                  .Returns(123456);

            var result = Subject.FreeSpaceOnDrives();

            result.Should().HaveCount(1);
        }

        [Test]
        public void should_return_two_drives_when_two_rootDirs_on_the_different_drive_exist()
        {
            Mocker.GetMock<IRootFolderRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<RootFolder> { new RootFolder { Id = 1, Path = @"C:\Test\TV" },
                                             new RootFolder { Id = 2, Path = @"D:\Test\TV" }});

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetPathRoot(@"C:\Test\TV"))
                  .Returns(@"C:\");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetPathRoot(@"D:\Test\TV"))
                  .Returns(@"D:\");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetAvailableSpace(It.IsAny<string>()))
                  .Returns(123456);

            var result = Subject.FreeSpaceOnDrives();

            result.Should().HaveCount(2);
        }

        [Test]
        public void should_skip_rootDir_if_not_found_on_disk()
        {
            Mocker.GetMock<IRootFolderRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<RootFolder> { new RootFolder { Id = 1, Path = @"C:\Test\TV" } });

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetPathRoot(@"C:\Test\TV"))
                  .Returns(@"C:\");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetAvailableSpace(It.IsAny<string>()))
                  .Throws(new DirectoryNotFoundException());

            var result = Subject.FreeSpaceOnDrives();

            result.Should().HaveCount(0);

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}