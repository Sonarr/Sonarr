using System.IO;
using System.Net;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.Blackhole;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests
{
    [TestFixture]
    public class BlackholeProviderFixture : CoreTest<Blackhole>
    {
        private const string _nzbUrl = "http://www.nzbs.com/url";
        private const string _title = "some_nzb_title";
        private string _blackHoleFolder;
        private string _nzbPath;
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _blackHoleFolder = @"c:\nzb\blackhole\".AsOsAgnostic();
            _nzbPath = @"c:\nzb\blackhole\some_nzb_title.nzb".AsOsAgnostic();

            _remoteEpisode = new RemoteEpisode();
            _remoteEpisode.Release = new ReleaseInfo();
            _remoteEpisode.Release.Title = _title;
            _remoteEpisode.Release.DownloadUrl = _nzbUrl;

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new FolderSettings
            {
                Folder = _blackHoleFolder
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
        public void DownloadNzb_should_download_file_if_it_doesnt_exist()
        {
            Subject.DownloadNzb(_remoteEpisode);

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(_nzbUrl, _nzbPath), Times.Once());
        }

        [Test]
        public void should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(_blackHoleFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV].nzb");
            _remoteEpisode.Release.Title = illegalTitle;

            Subject.DownloadNzb(_remoteEpisode);

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(It.IsAny<string>(), expectedFilename), Times.Once());
        }
    }
}
