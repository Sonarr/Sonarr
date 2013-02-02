using System.Linq;
using System;
using System.Diagnostics;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    internal class PostDownloadScanJobFixture : SqlCeTest
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<DiskProvider>().Setup(s => s.FolderExists(It.IsAny<string>())).Returns(true);
        }

        [Test]
        public void should_use_options_Path_when_provided()
        {
            var path = @"C:\Test\Unsorted TV";

            Mocker.GetMock<PostDownloadProvider>().Setup(s => s.ProcessDropFolder(path));
            Mocker.Resolve<PostDownloadScanJob>().Start(MockNotification, new { Path = path });

            Mocker.GetMock<PostDownloadProvider>().Verify(s => s.ProcessDropFolder(path), Times.Once());
        }

        [Test]
        public void should_not_get_sabDropDir_when_path_is_supplied()
        {
            var path = @"C:\Test\Unsorted TV";

            Mocker.GetMock<PostDownloadProvider>().Setup(s => s.ProcessDropFolder(path));
            Mocker.Resolve<PostDownloadScanJob>().Start(MockNotification, new { Path = path });

            Mocker.GetMock<ConfigProvider>().Verify(s => s.DownloadClientTvDirectory, Times.Never());
        }

        [Test]
        public void should_get_sabDropDir_when_path_is_not_supplied()
        {
            var path = @"C:\Test\Unsorted TV";

            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.DownloadClientTvDirectory).Returns(path);
            Mocker.Resolve<PostDownloadScanJob>().Start(MockNotification, null);

            Mocker.GetMock<ConfigProvider>().Verify(s => s.DownloadClientTvDirectory, Times.Once());
        }

        [Test]
        public void should_use_sabDropDir_when_options_Path_is_not_provided()
        {
            var path = @"C:\Test\Unsorted TV";

            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.DownloadClientTvDirectory).Returns(path);
            Mocker.Resolve<PostDownloadScanJob>().Start(MockNotification, null);

            Mocker.GetMock<PostDownloadProvider>().Verify(s => s.ProcessDropFolder(path), Times.Once());
        }
    }
}
