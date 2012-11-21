using System;
using System.Collections.Generic;
using System.IO;
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
    public class BlackholeProviderFixture : CoreTest
    {
        private const string nzbUrl = "http://www.nzbs.com/url";
        private const string title = "some_nzb_title";
        private const string blackHoleFolder = @"d:\nzb\blackhole\";
        private const string nzbPath = @"d:\nzb\blackhole\some_nzb_title.nzb";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.BlackholeDirectory).Returns(blackHoleFolder);
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
        public void DownloadNzb_should_download_file_if_it_doesnt_exist()
        {
            Mocker.Resolve<BlackholeProvider>().DownloadNzb(nzbUrl, title).Should().BeTrue();

            Mocker.GetMock<HttpProvider>().Verify(c => c.DownloadFile(nzbUrl, nzbPath),Times.Once());
        }

        [Test]
        public void DownloadNzb_not_download_file_if_it_doesn_exist()
        {
            WithExistingFile();

            Mocker.Resolve<BlackholeProvider>().DownloadNzb(nzbUrl, title).Should().BeTrue();

            Mocker.GetMock<HttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_on_failed_download()
        {
            WithFailedDownload();

            Mocker.Resolve<BlackholeProvider>().DownloadNzb(nzbUrl, title).Should().BeFalse();
            
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(blackHoleFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV].nzb");

            Mocker.Resolve<BlackholeProvider>().DownloadNzb(nzbUrl, illegalTitle).Should().BeTrue();

            Mocker.GetMock<HttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), expectedFilename), Times.Once());
        }
    }
}
