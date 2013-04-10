using System.IO;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests
{
    [TestFixture]
    public class BlackholeProviderFixture : CoreTest<BlackholeProvider>
    {
        private const string nzbUrl = "http://www.nzbs.com/url";
        private const string title = "some_nzb_title";
        private const string blackHoleFolder = @"d:\nzb\blackhole\";
        private const string nzbPath = @"d:\nzb\blackhole\some_nzb_title.nzb";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.BlackholeDirectory).Returns(blackHoleFolder);
        }


        private void WithExistingFile()
        {
            Mocker.GetMock<DiskProvider>().Setup(c => c.FileExists(nzbPath)).Returns(true);
        }

        private void WithFailedDownload()
        {
            Mocker.GetMock<IHttpProvider>().Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Throws(new WebException());
        }

        [Test]
        public void DownloadNzb_should_download_file_if_it_doesnt_exist()
        {
            Subject.DownloadNzb(nzbUrl, title, false).Should().BeTrue();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(nzbUrl, nzbPath), Times.Once());
        }

        [Test]
        public void DownloadNzb_not_download_file_if_it_doesn_exist()
        {
            WithExistingFile();

            Subject.DownloadNzb(nzbUrl, title, false).Should().BeTrue();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_on_failed_download()
        {
            WithFailedDownload();

            Subject.DownloadNzb(nzbUrl, title, false).Should().BeFalse();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(blackHoleFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV].nzb");

            Subject.DownloadNzb(nzbUrl, illegalTitle, false).Should().BeTrue();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), expectedFilename), Times.Once());
        }
    }
}
