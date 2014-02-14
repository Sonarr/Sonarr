using System;
using System.IO;
using System.Net;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.Pneumatic;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests
{
    [TestFixture]
    public class PneumaticProviderFixture : CoreTest<Pneumatic>
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

            Mocker.GetMock<IConfigService>().SetupGet(c => c.DownloadedEpisodesFolder).Returns(_sabDrop);

            _remoteEpisode = new RemoteEpisode();
            _remoteEpisode.Release = new ReleaseInfo();
            _remoteEpisode.Release.Title = _title;
            _remoteEpisode.Release.DownloadUrl = _nzbUrl;

            _remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = false;

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new FolderSettings
            {
                Folder = _pneumaticFolder
            };
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
            Subject.DownloadNzb(_remoteEpisode);

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(_nzbUrl, _nzbPath), Times.Once());
        }


        [Test]
        public void should_throw_on_failed_download()
        {
            WithFailedDownload();

            Assert.Throws<WebException>(() => Subject.DownloadNzb(_remoteEpisode));
        }

        [Test]
        public void should_throw_if_full_season_download()
        {
            _remoteEpisode.Release.Title = "30 Rock - Season 1";
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = true;

            Assert.Throws<NotImplementedException>(() => Subject.DownloadNzb(_remoteEpisode));
        }

        [Test]
        public void should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(_pneumaticFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV].nzb");
            _remoteEpisode.Release.Title = illegalTitle;

            Subject.DownloadNzb(_remoteEpisode);

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), expectedFilename), Times.Once());
        }
    }
}
