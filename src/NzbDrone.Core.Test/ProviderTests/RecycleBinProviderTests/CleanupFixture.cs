using System;
using System.IO;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.RecycleBinProviderTests
{
    [TestFixture]

    public class CleanupFixture : CoreTest
    {
        private const string RecycleBin = @"C:\Test\RecycleBin";

        private void WithExpired()
        {
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderGetLastWrite(It.IsAny<string>()))
                                            .Returns(DateTime.UtcNow.AddDays(-10));

            Mocker.GetMock<IDiskProvider>().Setup(s => s.FileGetLastWrite(It.IsAny<string>()))
                                            .Returns(DateTime.UtcNow.AddDays(-10));
        }

        private void WithNonExpired()
        {
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderGetLastWrite(It.IsAny<string>()))
                                            .Returns(DateTime.UtcNow.AddDays(-3));

            Mocker.GetMock<IDiskProvider>().Setup(s => s.FileGetLastWrite(It.IsAny<string>()))
                                            .Returns(DateTime.UtcNow.AddDays(-3));
        }

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(RecycleBin);
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBinCleanupDays).Returns(7);

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetDirectories(RecycleBin))
                    .Returns(new[] { @"C:\Test\RecycleBin\Folder1", @"C:\Test\RecycleBin\Folder2", @"C:\Test\RecycleBin\Folder3" });

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles(RecycleBin, SearchOption.AllDirectories))
                    .Returns(new[] { @"C:\Test\RecycleBin\File1.avi", @"C:\Test\RecycleBin\File2.mkv" });
        }

        [Test]
        public void should_return_if_recycleBin_not_configured()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(string.Empty);

            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetDirectories(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_if_recycleBinCleanupDays_is_zero()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBinCleanupDays).Returns(0);

            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetDirectories(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_all_expired_files()
        {
            WithExpired();
            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public void should_not_delete_all_non_expired_folders()
        {
            WithNonExpired();
            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void should_not_delete_all_non_expired_files()
        {
            WithNonExpired();
            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(It.IsAny<string>()), Times.Never());
        }
    }
}
