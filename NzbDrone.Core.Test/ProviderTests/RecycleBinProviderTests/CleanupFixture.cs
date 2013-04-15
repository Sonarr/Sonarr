using System;
using System.IO;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Providers;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.RecycleBinProviderTests
{
    [TestFixture]
    
    public class CleanupFixture : CoreTest
    {
        private const string RecycleBin = @"C:\Test\RecycleBin";

        private void WithExpired()
        {
            Mocker.GetMock<DiskProvider>().Setup(s => s.GetLastFolderWrite(It.IsAny<String>()))
                                            .Returns(DateTime.UtcNow.AddDays(-10));

            Mocker.GetMock<DiskProvider>().Setup(s => s.GetLastFileWrite(It.IsAny<String>()))
                                            .Returns(DateTime.UtcNow.AddDays(-10));
        }

        private void WithNonExpired()
        {
            Mocker.GetMock<DiskProvider>().Setup(s => s.GetLastFolderWrite(It.IsAny<String>()))
                                            .Returns(DateTime.UtcNow.AddDays(-3));

            Mocker.GetMock<DiskProvider>().Setup(s => s.GetLastFileWrite(It.IsAny<String>()))
                                            .Returns(DateTime.UtcNow.AddDays(-3));
        }

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(RecycleBin);

            Mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectories(RecycleBin))
                    .Returns(new [] { @"C:\Test\RecycleBin\Folder1", @"C:\Test\RecycleBin\Folder2", @"C:\Test\RecycleBin\Folder3" });

            Mocker.GetMock<DiskProvider>().Setup(s => s.GetFiles(RecycleBin, SearchOption.TopDirectoryOnly))
                    .Returns(new [] { @"C:\Test\RecycleBin\File1.avi", @"C:\Test\RecycleBin\File2.mkv" });
        }

        [Test]
        public void should_return_if_recycleBin_not_configured()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(String.Empty);

            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<DiskProvider>().Verify(v => v.GetDirectories(It.IsAny<String>()), Times.Never());
        }

        [Test]
        public void should_delete_all_expired_folders()
        {          
            WithExpired();
            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<DiskProvider>().Verify(v => v.DeleteFolder(It.IsAny<String>(), true), Times.Exactly(3));
        }

        [Test]
        public void should_delete_all_expired_files()
        {
            WithExpired();
            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<DiskProvider>().Verify(v => v.DeleteFile(It.IsAny<String>()), Times.Exactly(2));
        }

        [Test]
        public void should_not_delete_all_non_expired_folders()
        {
            WithNonExpired();
            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<DiskProvider>().Verify(v => v.DeleteFolder(It.IsAny<String>(), true), Times.Never());
        }

        [Test]
        public void should_not_delete_all_non_expired_files()
        {
            WithNonExpired();
            Mocker.Resolve<RecycleBinProvider>().Cleanup();

            Mocker.GetMock<DiskProvider>().Verify(v => v.DeleteFile(It.IsAny<String>()), Times.Never());
        }
    }
}
