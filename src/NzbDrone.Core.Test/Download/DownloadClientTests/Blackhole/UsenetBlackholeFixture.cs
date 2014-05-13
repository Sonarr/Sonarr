using System;
using System.IO;
using System.Net;
using System.Linq;
using Moq;
using NUnit.Framework;
using FluentAssertions;
using NzbDrone.Test.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.UsenetBlackhole;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.Blackhole
{

    [TestFixture]
    public class UsenetBlackholeFixture : DownloadClientFixtureBase<UsenetBlackhole>
    {
        protected string _completedDownloadFolder;
        protected string _blackholeFolder;
        protected string _filePath;

        [SetUp]
        public void Setup()
        {
            _completedDownloadFolder = @"c:\blackhole\completed".AsOsAgnostic();
            _blackholeFolder = @"c:\blackhole\nzb".AsOsAgnostic();
            _filePath = (@"c:\blackhole\nzb\" + _title + ".nzb").AsOsAgnostic();

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new UsenetBlackholeSettings
            {
                NzbFolder = _blackholeFolder,
                WatchFolder = _completedDownloadFolder
            };
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()))
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

            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(_downloadUrl, _filePath), Times.Once());
        }

        [Test]
        public void Download_should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(_blackholeFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV]" + Path.GetExtension(_filePath));

            var remoteEpisode = CreateRemoteEpisode();
            remoteEpisode.Release.Title = illegalTitle;

            Subject.Download(remoteEpisode);

            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), expectedFilename), Times.Once());
        }

        [Test]
        public void GetItems_should_considered_locked_files_downloading()
        {
            GivenCompletedItem();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.IsFileLocked(It.IsAny<string>()))
                .Returns(true);

            var result = Subject.GetItems().Single();

            result.Status.Should().Be(DownloadItemStatus.Downloading);
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
