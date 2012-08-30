using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.DownloadClients;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.DownloadClientTests
{
    [TestFixture]
    public class PneumaticProviderFixture : CoreTest
    {
        private const string nzbUrl = "http://www.nzbs.com/url";
        private const string title = "some_nzb_title";
        private const string pneumaticFolder = @"d:\nzb\pneumatic\";
        private const string nzbPath = @"d:\nzb\blackhole\some_nzb_title.nzb";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.BlackholeDirectory).Returns(pneumaticFolder);
        }

        private void WithExistingFile()
        {
            Mocker.GetMock<DiskProvider>().Setup(c => c.FileExists(nzbPath)).Returns(true);
        }

        private void WithFailedDownload()
        {
            Mocker.GetMock<HttpProvider>().Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Throws(new WebException());
        }

        [Test]
        public void should_download_file_if_it_doesnt_exist()
        {
            Mocker.Resolve<PneumaticProvider>().DownloadNzb(nzbUrl, title).Should().BeTrue();

            Mocker.GetMock<HttpProvider>().Verify(c => c.DownloadFile(nzbUrl, nzbPath),Times.Once());
        }

        [Test]
        public void should_not_download_file_if_it_doesn_exist()
        {
            WithExistingFile();

            Mocker.Resolve<PneumaticProvider>().DownloadNzb(nzbUrl, title).Should().BeTrue();

            Mocker.GetMock<HttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_on_failed_download()
        {
            WithFailedDownload();

            Mocker.Resolve<PneumaticProvider>().DownloadNzb(nzbUrl, title).Should().BeFalse();
            
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_skip_if_full_season_download()
        {
            Mocker.Resolve<PneumaticProvider>().DownloadNzb(nzbUrl, "30 Rock - Season 1").Should().BeFalse();
            ExceptionVerification.ExpectedErrors(1);
        }


    }
}
