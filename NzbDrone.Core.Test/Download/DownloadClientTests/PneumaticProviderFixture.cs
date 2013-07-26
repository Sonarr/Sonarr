using System.IO;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests
{
    [TestFixture]
    public class PneumaticProviderFixture : CoreTest<PneumaticClient>
    {
        private const string _nzbUrl = "http://www.nzbs.com/url";
        private const string _title = "30.Rock.S01E05.hdtv.xvid-LoL";
        private string _pneumaticFolder;
        private string _sabDrop;
        private string _nzbPath;
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _pneumaticFolder = @"d:\nzb\pneumatic\".AsOsAgnostic();

            _nzbPath = Path.Combine(_pneumaticFolder, _title + ".nzb").AsOsAgnostic();
            _sabDrop = @"d:\unsorted tv\".AsOsAgnostic();

            Mocker.GetMock<IConfigService>().SetupGet(c => c.PneumaticFolder).Returns(_pneumaticFolder);
            Mocker.GetMock<IConfigService>().SetupGet(c => c.DownloadedEpisodesFolder).Returns(_sabDrop);

            _remoteEpisode = new RemoteEpisode();
            _remoteEpisode.Report = new ReportInfo();
            _remoteEpisode.Report.Title = _title;
            _remoteEpisode.Report.NzbUrl = _nzbUrl;

            _remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = false;
        }

        private void WithExistingFile()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileExists(_nzbPath)).Returns(true);
        }

        private void WithFailedDownload()
        {
            Mocker.GetMock<IHttpProvider>().Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Throws(new WebException());
        }

        [Test]
        public void should_download_file_if_it_doesnt_exist()
        {
            Subject.DownloadNzb(_remoteEpisode).Should().BeTrue();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(_nzbUrl, _nzbPath), Times.Once());
        }

        [Test]
        public void should_not_download_file_if_it_doesn_exist()
        {
            WithExistingFile();

            Subject.DownloadNzb(_remoteEpisode).Should().BeTrue();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_on_failed_download()
        {
            WithFailedDownload();

            Subject.DownloadNzb(_remoteEpisode).Should().BeFalse();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_skip_if_full_season_download()
        {
            _remoteEpisode.Report.Title = "30 Rock - Season 1";
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = true;

            Mocker.Resolve<PneumaticClient>().DownloadNzb(_remoteEpisode).Should().BeFalse();
        }

        [Test]
        public void should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(_pneumaticFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV].nzb");
            _remoteEpisode.Report.Title = illegalTitle;

            Subject.DownloadNzb(_remoteEpisode).Should().BeTrue();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), expectedFilename), Times.Once());
        }
    }
}
