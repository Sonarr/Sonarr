// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;
using PetaPoco;

namespace NzbDrone.Core.Test.ProviderTests.RootDirProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class FreeSpaceOnDrivesFixture : SqlCeTest
    {
        [Test]
        public void should_return_one_drive_when_only_one_root_dir_exists()
        {
            Mocker.GetMock<IDatabase>()
                  .Setup(s => s.Fetch<RootDir>())
                  .Returns(new List<RootDir> { new RootDir { Id = 1, Path = @"C:\Test\TV" } });

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.GetPathRoot(@"C:\Test\TV"))
                  .Returns(@"C:\");

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.FreeDiskSpace(new DirectoryInfo(@"C:\")))
                  .Returns(123456);

            var result = Mocker.Resolve<RootFolderService>().FreeSpaceOnDrives();

            result.Should().HaveCount(1);
        }

        [Test]
        public void should_return_one_drive_when_two_rootDirs_on_the_same_drive_exist()
        {
            Mocker.GetMock<IDatabase>()
                  .Setup(s => s.Fetch<RootDir>())
                  .Returns(new List<RootDir> { new RootDir { Id = 1, Path = @"C:\Test\TV" },
                                             new RootDir { Id = 2, Path = @"C:\Test\TV2" }});

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.GetPathRoot(It.IsAny<String>()))
                  .Returns(@"C:\");

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.FreeDiskSpace(new DirectoryInfo(@"C:\")))
                  .Returns(123456);

            var result = Mocker.Resolve<RootFolderService>().FreeSpaceOnDrives();

            result.Should().HaveCount(1);
        }

        [Test]
        public void should_return_two_drives_when_two_rootDirs_on_the_different_drive_exist()
        {
            Mocker.GetMock<IDatabase>()
                  .Setup(s => s.Fetch<RootDir>())
                  .Returns(new List<RootDir> { new RootDir { Id = 1, Path = @"C:\Test\TV" },
                                             new RootDir { Id = 2, Path = @"D:\Test\TV" }});

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.GetPathRoot(@"C:\Test\TV"))
                  .Returns(@"C:\");

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.GetPathRoot(@"D:\Test\TV"))
                  .Returns(@"D:\");

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.FreeDiskSpace(It.IsAny<DirectoryInfo>()))
                  .Returns(123456);

            var result = Mocker.Resolve<RootFolderService>().FreeSpaceOnDrives();

            result.Should().HaveCount(2);
        }

        [Test]
        public void should_skip_rootDir_if_not_found_on_disk()
        {
            Mocker.GetMock<IDatabase>()
                  .Setup(s => s.Fetch<RootDir>())
                  .Returns(new List<RootDir> { new RootDir { Id = 1, Path = @"C:\Test\TV" } });

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.GetPathRoot(@"C:\Test\TV"))
                  .Returns(@"C:\");

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.FreeDiskSpace(It.IsAny<DirectoryInfo>()))
                  .Throws(new DirectoryNotFoundException());

            var result = Mocker.Resolve<RootFolderService>().FreeSpaceOnDrives();

            result.Should().HaveCount(0);

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}