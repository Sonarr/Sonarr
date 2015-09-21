using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands.Series;
using NzbDrone.Core.MediaFiles.Imports;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class DownloadedMediaCommandServiceFixture : CoreTest<DownloadedMediaCommandService>
    {
        private string _droneFactory = "c:\\drop\\".AsOsAgnostic();
        private string _downloadFolder = "c:\\drop_other\\Show.S01E01\\".AsOsAgnostic();

        private TrackedDownload _trackedDownload;
        private TrackedDownload _trackedMovieDownload;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IConfigService>().SetupGet(c => c.DownloadedEpisodesFolder)
                  .Returns(_droneFactory);

            Mocker.GetMock<IDownloadedMediaImportService>()
                .Setup(v => v.ProcessRootFolder(It.IsAny<DirectoryInfo>()))
                .Returns(new List<ImportResult>());

            Mocker.GetMock<IDownloadedMediaImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>());

            Mocker.GetMock<IDownloadedMediaImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>());

            var downloadItem = Builder<DownloadClientItem>.CreateNew()
                .With(v => v.DownloadId = "sab1")
                .With(v => v.Status = DownloadItemStatus.Downloading)
                .Build();

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                .With(v => v.Series = new Series())
                .Build();

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                .With(v => v.Movie = new Movie())
                .Build();

            _trackedDownload = new TrackedDownload
                    {
                        DownloadItem = downloadItem,
                        RemoteItem = remoteEpisode,
                        State = TrackedDownloadStage.Downloading
                    };

            _trackedMovieDownload = new TrackedDownload
            {
                DownloadItem = downloadItem,
                RemoteItem = remoteMovie,
                State = TrackedDownloadStage.Downloading
            };
        }

        private void GivenValidSeriesQueueItem()
        {
            Mocker.GetMock<ITrackedDownloadService>()
                  .Setup(s => s.Find("sab1"))
                  .Returns(_trackedDownload);
        }

        private void GivenValidMovieQueueItem()
        {
            Mocker.GetMock<ITrackedDownloadService>()
                  .Setup(s => s.Find("sab1"))
                  .Returns(_trackedMovieDownload);
        }

        [Test]
        public void should_process_dronefactory_if_path_is_not_specified()
        {
            Subject.Execute(new DownloadedEpisodesScanCommand());

            Mocker.GetMock<IDownloadedMediaImportService>().Verify(c => c.ProcessRootFolder(It.IsAny<DirectoryInfo>()), Times.Once());
        }

        [Test]
        public void should_skip_import_if_dronefactory_doesnt_exist()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(It.IsAny<string>())).Returns(false);

            Subject.Execute(new DownloadedEpisodesScanCommand());

            Mocker.GetMock<IDownloadedMediaImportService>().Verify(c => c.ProcessRootFolder(It.IsAny<DirectoryInfo>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_ignore_downloadclientid_if_path_is_not_specified()
        {
            Subject.Execute(new DownloadedEpisodesScanCommand() { DownloadClientId = "sab1" });

            Mocker.GetMock<IDownloadedMediaImportService>().Verify(c => c.ProcessRootFolder(It.IsAny<DirectoryInfo>()), Times.Once());
        }

        [Test]
        public void should_process_folder_if_downloadclientid_is_not_specified()
        {
            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFolder });

            Mocker.GetMock<IDownloadedMediaImportService>().Verify(c => c.ProcessPath(It.IsAny<string>(), null, null), Times.Once());
        }

        [Test]
        public void should_process_folder_with_downloadclientitem_if_available()
        {
            GivenValidSeriesQueueItem();

            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFolder, DownloadClientId = "sab1" });

            Mocker.GetMock<IDownloadedMediaImportService>().Verify(c => c.ProcessPath(_downloadFolder, _trackedDownload.RemoteItem.Media, _trackedDownload.DownloadItem), Times.Once());
        }

        [Test]
        public void should_process_folder_with_downloadclientitem_if_available_movie()
        {
            GivenValidMovieQueueItem();

            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFolder, DownloadClientId = "sab1" });

            Mocker.GetMock<IDownloadedMediaImportService>().Verify(c => c.ProcessPath(_downloadFolder, _trackedMovieDownload.RemoteItem.Media, _trackedMovieDownload.DownloadItem), Times.Once());
        }

        [Test]
        public void should_process_folder_without_downloadclientitem_if_not_available()
        {
            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFolder, DownloadClientId = "sab1" });

            Mocker.GetMock<IDownloadedMediaImportService>().Verify(c => c.ProcessPath(_downloadFolder, null, null), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}