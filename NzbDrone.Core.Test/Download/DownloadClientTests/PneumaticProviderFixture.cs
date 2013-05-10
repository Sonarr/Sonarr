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
    public class PneumaticProviderFixture : CoreTest
    {
        private const string nzbUrl = "http://www.nzbs.com/url";
        private const string title = "30.Rock.S01E05.hdtv.xvid-LoL";
        private const string pneumaticFolder = @"d:\nzb\pneumatic\";
        private const string sabDrop = @"d:\unsorted tv\";
        private string nzbPath;

        [SetUp]
        public void Setup()
        {
            nzbPath = pneumaticFolder + title + ".nzb";

            Mocker.GetMock<IConfigService>().SetupGet(c => c.PneumaticDirectory).Returns(pneumaticFolder);
            Mocker.GetMock<IConfigService>().SetupGet(c => c.DownloadClientTvDirectory).Returns(sabDrop);
        }

        private void WithExistingFile()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileExists(nzbPath)).Returns(true);
        }

        private void WithFailedDownload()
        {
            Mocker.GetMock<IHttpProvider>().Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Throws(new WebException());
        }

        [Test]
        public void should_download_file_if_it_doesnt_exist()
        {
            Mocker.Resolve<PneumaticClient>().DownloadNzb(nzbUrl, title, false).Should().BeTrue();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(nzbUrl, nzbPath),Times.Once());
        }

        [Test]
        public void should_not_download_file_if_it_doesn_exist()
        {
            WithExistingFile();

            Mocker.Resolve<PneumaticClient>().DownloadNzb(nzbUrl, title, false).Should().BeTrue();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_on_failed_download()
        {
            WithFailedDownload();

            Mocker.Resolve<PneumaticClient>().DownloadNzb(nzbUrl, title, false).Should().BeFalse();
            
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_skip_if_full_season_download()
        {
            Mocker.Resolve<PneumaticClient>().DownloadNzb(nzbUrl, "30 Rock - Season 1", false).Should().BeFalse();
        }

        [Test]
        public void should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(pneumaticFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV].nzb");

            Mocker.Resolve<PneumaticClient>().DownloadNzb(nzbUrl, illegalTitle, false).Should().BeTrue();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), expectedFilename), Times.Once());
        }
    }
}
