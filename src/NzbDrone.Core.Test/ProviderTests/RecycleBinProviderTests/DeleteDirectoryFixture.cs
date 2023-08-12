using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.RecycleBinProviderTests
{
    [TestFixture]

    public class DeleteDirectoryFixture : CoreTest
    {
        private void WithRecycleBin()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(@"C:\Test\Recycle Bin".AsOsAgnostic());
        }

        private void WithoutRecycleBin()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(string.Empty);
        }

        [Test]
        public void should_use_delete_when_recycleBin_is_not_configured()
        {
            WithoutRecycleBin();

            var path = @"C:\Test\TV\30 Rock".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFolder(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFolder(path, true), Times.Once());
        }

        [Test]
        public void should_use_move_when_recycleBin_is_configured()
        {
            WithRecycleBin();

            var path = @"C:\Test\TV\30 Rock".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFolder(path);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(path, @"C:\Test\Recycle Bin\30 Rock".AsOsAgnostic(), TransferMode.Move), Times.Once());
        }

        [Test]
        public void should_call_directorySetLastWriteTime()
        {
            WithRecycleBin();

            var path = @"C:\Test\TV\30 Rock".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFolder(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.FolderSetLastWriteTime(@"C:\Test\Recycle Bin\30 Rock".AsOsAgnostic(), It.IsAny<DateTime>()), Times.Once());
        }

        [Test]
        public void should_call_fileSetLastWriteTime_for_each_file()
        {
            WindowsOnly();
            WithRecycleBin();
            var path = @"C:\Test\TV\30 Rock".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles(@"C:\Test\Recycle Bin\30 Rock".AsOsAgnostic(), true))
                                           .Returns(new[] { "File1", "File2", "File3" });

            Mocker.Resolve<RecycleBinProvider>().DeleteFolder(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.FileSetLastWriteTime(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(3));
        }
    }
}
