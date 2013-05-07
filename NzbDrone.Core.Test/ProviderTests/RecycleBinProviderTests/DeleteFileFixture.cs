

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;

using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.RecycleBinProviderTests
{
    [TestFixture]
    
    public class DeleteFileFixture : CoreTest
    {
        private void WithRecycleBin()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(@"C:\Test\Recycle Bin");
        }

        private void WithoutRecycleBin()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(String.Empty);
        }

        [Test]
        public void should_use_delete_when_recycleBin_is_not_configured()
        {
            WithoutRecycleBin();

            var path = @"C:\Test\TV\30 Rock\S01E01.avi";

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<DiskProvider>().Verify(v => v.DeleteFile(path), Times.Once());
        }

        [Test]
        public void should_use_move_when_recycleBin_is_configured()
        {
            WithRecycleBin();

            var path = @"C:\Test\TV\30 Rock\S01E01.avi";

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<DiskProvider>().Verify(v => v.MoveFile(path, @"C:\Test\Recycle Bin\S01E01.avi"), Times.Once());
        }

        [Test]
        public void should_call_fileSetLastWriteTime_for_each_file()
        {
            WithRecycleBin();
            var path = @"C:\Test\TV\30 Rock\S01E01.avi";


            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<DiskProvider>().Verify(v => v.FileSetLastWriteTimeUtc(@"C:\Test\Recycle Bin\S01E01.avi", It.IsAny<DateTime>()), Times.Once());
        }
    }
}
