using System;
using System.IO;
using System.Linq;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.TorrentBlackhole;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.Blackhole
{
    [TestFixture]
    public class TorrentBlackholeFixture : DownloadClientFixtureBase<TorrentBlackhole>
    {
        protected string _completedDownloadFolder;
        protected string _blackholeFolder;
        protected string _filePath;

        [SetUp]
        public void Setup()
        {
            _completedDownloadFolder = @"c:\blackhole\completed".AsOsAgnostic();
            _blackholeFolder = @"c:\blackhole\torrent".AsOsAgnostic();
            _filePath = (@"c:\blackhole\torrent\" + _title + ".torrent").AsOsAgnostic();

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new TorrentBlackholeSettings
            {
                TorrentFolder = _blackholeFolder,
                WatchFolder = _completedDownloadFolder
            };

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.OpenWriteStream(It.IsAny<string>()))
                .Returns(() => new FileStream(GetTempFilePath(), FileMode.Create));

            Mocker.GetMock<ITorrentFileInfoReader>()
                .Setup(c => c.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                .Returns("myhash");
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                .Throws(new WebException());
        }

        protected void GivenCompletedItem()
        {
            var targetDir = Path.Combine(_completedDownloadFolder, _title);
            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetDirectories(_completedDownloadFolder))
                .Returns(new[] { targetDir });

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetFiles(targetDir, SearchOption.AllDirectories))
                .Returns(new[] { Path.Combine(_completedDownloadFolder, "somefile.mkv") });

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetFileSize(It.IsAny<string>()))
                .Returns(1000000);
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            GivenCompletedItem();

            var result = Subject.GetItems().Single();

            VerifyCompleted(result);
        }

        [Test]
        public void should_return_category()
        {
            GivenCompletedItem();

            var result = Subject.GetItems().Single();

            // We must have a category or CDH won't pick it up.
            result.Category.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void Download_should_download_file_if_it_doesnt_exist()
        {
            var remoteEpisode = CreateRemoteEpisode();

            Subject.Download(remoteEpisode);

            Mocker.GetMock<IHttpClient>().Verify(c => c.Get(It.Is<HttpRequest>(v => v.Url.ToString() == _downloadUrl)), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_filePath), Times.Once());
            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Download_should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(_blackholeFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV]" + Path.GetExtension(_filePath));

            var remoteEpisode = CreateRemoteEpisode();
            remoteEpisode.Release.Title = illegalTitle;

            Subject.Download(remoteEpisode);

            Mocker.GetMock<IHttpClient>().Verify(c => c.Get(It.Is<HttpRequest>(v => v.Url.ToString() == _downloadUrl)), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(expectedFilename), Times.Once());
            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void GetItems_should_considered_locked_files_queued()
        {
            GivenCompletedItem();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.IsFileLocked(It.IsAny<string>()))
                .Returns(true);

            var items = Subject.GetItems().ToList();

            items.Count.Should().Be(1);
            items.First().Status.Should().Be(DownloadItemStatus.Downloading);
        }

        [Test]
        public void RemoveItem_should_delete_file()
        {
            GivenCompletedItem();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(true);

            Subject.RemoveItem("_Droned.S01E01.Pilot.1080p.WEB-DL-DRONE_0", true);

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFile(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void RemoveItem_should_delete_directory()
        {
            GivenCompletedItem();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<string>()))
                .Returns(true);

            Subject.RemoveItem("_Droned.S01E01.Pilot.1080p.WEB-DL-DRONE_0", true);

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Once());
        }

        [Test]
        public void RemoveItem_should_ignore_if_unknown_item()
        {
            Subject.RemoveItem("_Droned.S01E01.Pilot.1080p.WEB-DL-DRONE_0", true);

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFile(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void RemoveItem_should_throw_if_deleteData_is_false()
        {
            GivenCompletedItem();

            Assert.Throws<NotSupportedException>(() => Subject.RemoveItem("_Droned.S01E01.Pilot.1080p.WEB-DL-DRONE_0", false));

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFile(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void should_return_status_with_outputdirs()
        {
            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(_completedDownloadFolder);
        }
    }
}
