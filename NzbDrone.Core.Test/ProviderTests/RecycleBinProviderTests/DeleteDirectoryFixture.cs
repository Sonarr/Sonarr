// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;
using PetaPoco;

namespace NzbDrone.Core.Test.ProviderTests.RecycleBinProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class DeleteDirectoryFixture : CoreTest
    {
        private void WithRecycleBin()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.RecycleBin).Returns(@"C:\Test\Recycle Bin");
        }

        private void WithoutRecycleBin()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.RecycleBin).Returns(String.Empty);
        }

        [Test]
        public void should_use_delete_when_recycleBin_is_not_configured()
        {
            WithoutRecycleBin();

            var path = @"C:\Test\TV\30 Rock";

            Mocker.Resolve<RecycleBinProvider>().DeleteDirectory(path);

            Mocker.GetMock<DiskProvider>().Verify(v => v.DeleteFolder(path, true), Times.Once());
        }

        [Test]
        public void should_use_move_when_recycleBin_is_configured()
        {
            WithRecycleBin();

            var path = @"C:\Test\TV\30 Rock";

            Mocker.Resolve<RecycleBinProvider>().DeleteDirectory(path);

            Mocker.GetMock<DiskProvider>().Verify(v => v.MoveDirectory(path, @"C:\Test\Recycle Bin\30 Rock"), Times.Once());
        }

        [Test]
        public void should_call_directorySetLastWriteTime()
        {
            WithRecycleBin();

            var path = @"C:\Test\TV\30 Rock";

            Mocker.Resolve<RecycleBinProvider>().DeleteDirectory(path);

            Mocker.GetMock<DiskProvider>().Verify(v => v.DirectorySetLastWriteTimeUtc(@"C:\Test\Recycle Bin\30 Rock", It.IsAny<DateTime>()), Times.Once());
        }

        [Test]
        public void should_call_fileSetLastWriteTime_for_each_file()
        {
            WithRecycleBin();
            var path = @"C:\Test\TV\30 Rock";

            Mocker.GetMock<DiskProvider>().Setup(s => s.GetFiles(@"C:\Test\Recycle Bin\30 Rock", SearchOption.AllDirectories))
                                            .Returns(new[]{ "File1", "File2", "File3" });

            Mocker.Resolve<RecycleBinProvider>().DeleteDirectory(path);

            Mocker.GetMock<DiskProvider>().Verify(v => v.FileSetLastWriteTimeUtc(It.IsAny<String>(), It.IsAny<DateTime>()), Times.Exactly(3));
        }
    }
}
