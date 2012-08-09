// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.PostDownloadProviderTests
{
    [TestFixture]
    public class ProcessDownloadProviderFixture : CoreTest
    {
        Series fakeSeries;

        [SetUp]
        public void Setup()
        {
            fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Path = @"C:\Test\TV\30 Rock")
                .Build();
        }

        private void WithOldWrite()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetLastDirectoryWrite(It.IsAny<String>()))
                .Returns(DateTime.Now.AddDays(-5));
        }

        private void WithRecentWrite()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetLastDirectoryWrite(It.IsAny<String>()))
                .Returns(DateTime.UtcNow);
        }

        private void WithValidSeries()
        {
            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.FindSeries(It.IsAny<string>()))
                .Returns(fakeSeries);
        }

        private void WithImportableFiles()
        {
            Mocker.GetMock<DiskScanProvider>()
                .Setup(c => c.Scan(It.IsAny<Series>(), It.IsAny<string>()))
                .Returns(Builder<EpisodeFile>.CreateListOfSize(1).Build().ToList());
        }

        private void WithLotsOfFreeDiskSpace()
        {
            Mocker.GetMock<DiskProvider>().Setup(s => s.FreeDiskSpace(It.IsAny<DirectoryInfo>())).Returns(1000000000);
        }

        private void WithImportedFiles(string droppedFolder)
        {
            var fakeEpisodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .All()
                .With(f => f.SeriesId = fakeSeries.SeriesId)
                .Build().ToList();

            Mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, droppedFolder)).Returns(fakeEpisodeFiles);
        }

        [Test]
        public void should_skip_if_folder_is_tagged_and_too_fresh()
        {
            WithStrictMocker();
            WithRecentWrite();

            var droppedFolder = new DirectoryInfo(TempFolder + "\\_test\\");
            droppedFolder.Create();

            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);
        }

        [Test]
        public void should_continue_processing_if_folder_is_tagged_and_not_fresh()
        {
            WithOldWrite();

            var droppedFolder = new DirectoryInfo(TempFolder + "\\_test\\");
            droppedFolder.Create();

            //Act
            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries(It.IsAny<String>())).Returns<Series>(null).Verifiable();
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            Mocker.VerifyAllMocks();
            ExceptionVerification.IgnoreWarns();
        }


        [Test]
        public void should_search_for_series_using_title_without_status()
        {
            WithOldWrite();

            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\_unpack_The Office - S01E01 - Episode Title");

            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns<Series>(null).Verifiable();

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            Mocker.VerifyAllMocks();
            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_search_for_series_using_folder_name()
        {
            WithOldWrite();
            WithValidSeries();
            WithImportableFiles();
            
            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - S01E01 - Episode Title");
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            Mocker.GetMock<DiskScanProvider>()
                .Verify(c=>c.Scan(fakeSeries, It.IsAny<string>()));
            
        }

        [Test]
        public void should_search_for_series_using_file_name()
        {
            WithOldWrite();
            WithValidSeries();
            WithImportableFiles();

            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - S01E01 - Episode Title");
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            Mocker.GetMock<DiskScanProvider>()
                .Verify(c => c.Scan(fakeSeries, It.IsAny<string>()));

        }

        [Test]
        [Ignore("Disabled tagging")]
        public void when_series_isnt_found_folder_should_be_tagged_as_unknown_series()
        {
            //Setup
            WithStrictMocker();
            WithOldWrite();
            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - S01E01 - Episode Title");

            var taggedFolder = @"C:\Test\Unsorted TV\_UnknownSeries_The Office - S01E01 - Episode Title";

            //Act
            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns<Series>(null);
            Mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(droppedFolder.FullName, taggedFolder));

            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            Mocker.VerifyAllMocks();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        [Ignore("Disabled tagging")]
        public void when_no_files_are_imported_folder_should_be_tagged_with_parse_error()
        {
            //Setup
            WithStrictMocker();
            WithOldWrite();
            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - S01E01 - Episode Title");

            var taggedFolder = @"C:\Test\Unsorted TV\_ParseError_The Office - S01E01 - Episode Title";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "The Office")
                .Build();

            //Act
            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(fakeSeries);
            Mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, droppedFolder.FullName)).Returns(new List<EpisodeFile>());
            Mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(droppedFolder.FullName, taggedFolder));
            Mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectorySize(droppedFolder.FullName)).Returns(Constants.IgnoreFileSize + 10.Megabytes());


            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            Mocker.VerifyAllMocks();
            ExceptionVerification.ExpectedWarns(1);
        }


        [Test]
        [Ignore("Disabled tagging")]
        public void when_no_file_are_imported_and_folder_size_isnt_small_enought_folder_should_be_tagged_unknown()
        {
            //Setup
            WithStrictMocker();
            WithOldWrite();
            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - Season 01");

            var taggedFolder = PostDownloadProvider.GetTaggedFolderName(droppedFolder, PostDownloadStatusType.Unknown);

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "The Office")
                .Build();

            var fakeEpisodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .All()
                .With(f => f.SeriesId = fakeSeries.SeriesId)
                .Build().ToList();

            //Act
            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(fakeSeries);
            Mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(droppedFolder.FullName, taggedFolder));
            Mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectorySize(droppedFolder.FullName)).Returns(Constants.IgnoreFileSize + 10.Megabytes());
            Mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, droppedFolder.FullName)).Returns(fakeEpisodeFiles);
            Mocker.GetMock<DiskScanProvider>().Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), true)).Returns(new EpisodeFile());

            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            Mocker.VerifyAllMocks();
            ExceptionVerification.ExpectedWarns(1);
        }

        [TestCase(@"\_UnknownSeries_The Office - S01E01 - Episode Title")]
        [TestCase(@"\_UnknownSeries_The Office - S01E01 - Episode Title\")]
        [TestCase("\\Test\\_UnknownSeries_The Office - S01E01 - Episode Title\\")]
        [TestCase("\\Test\\_UnknownSeries_The Office - S01E01 - Episode Title")]
        public void folder_shouldnt_be_tagged_with_same_tag_again(string path)
        {
            //Setup

            var droppedFolder = new DirectoryInfo(TempFolder + path);
            droppedFolder.Create();
            WithOldWrite();

            //Act
            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries(It.IsAny<String>())).Returns<Series>(null);
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<DiskProvider>().Verify(c => c.MoveDirectory(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void folder_should_not_be_tagged_if_existing_tag_is_diffrent()
        {
            //Setup
            WithOldWrite();
            var droppedFolder = new DirectoryInfo(TempFolder + @"\_UnknownEpisode_The Office - S01E01 - Episode Title");
            droppedFolder.Create();
            droppedFolder.LastWriteTime = DateTime.Now.AddHours(-1);

            var taggedFolder = TempFolder + @"\_UnknownSeries_The Office - S01E01 - Episode Title";

            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries(It.IsAny<String>())).Returns<Series>(null);

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<DiskProvider>().Verify(c => c.MoveDirectory(droppedFolder.FullName, taggedFolder), Times.Never());
            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void when_files_are_imported_and_folder_is_small_enough_dir_should_be_deleted()
        {
            //Setup
            WithStrictMocker();
            WithLotsOfFreeDiskSpace();

            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - Season 01");

            WithImportedFiles(droppedFolder.FullName);

            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(fakeSeries);
            Mocker.GetMock<DiskScanProvider>().Setup(s => s.CleanUpDropFolder(droppedFolder.FullName));
            Mocker.GetMock<DiskScanProvider>().Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), true)).Returns(new EpisodeFile());
            Mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectorySize(droppedFolder.FullName)).Returns(Constants.IgnoreFileSize - 1.Megabytes());
            Mocker.GetMock<DiskProvider>().Setup(s => s.DeleteFolder(droppedFolder.FullName, true));
            Mocker.GetMock<MetadataProvider>().Setup(s => s.CreateForEpisodeFiles(It.IsAny<List<EpisodeFile>>()));

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void all_imported_files_should_be_moved()
        {
            var droppedFolder = new DirectoryInfo(TempFolder);

            var fakeSeries = Builder<Series>.CreateNew()
                .Build();

            var fakeEpisodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .Build().ToList();

            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries(It.IsAny<string>())).Returns(fakeSeries);
            Mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, droppedFolder.FullName)).Returns(fakeEpisodeFiles);

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.MoveEpisodeFile(It.IsAny<EpisodeFile>(), true),
                Times.Exactly(fakeEpisodeFiles.Count));
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void ProcessDropFolder_should_only_process_folders_that_arent_known_series_folders()
        {
            WithLotsOfFreeDiskSpace();

            var subFolders = new[]
                                 {
                                    @"c:\drop\episode1",
                                    @"c:\drop\episode2",
                                    @"c:\drop\episode3",
                                    @"c:\drop\episode4"
                                 };

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetDirectories(It.IsAny<String>()))
                .Returns(subFolders);

            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.SeriesPathExists(subFolders[1]))
                .Returns(true);

            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.FindSeries(It.IsAny<String>()))
                .Returns(fakeSeries);

            Mocker.GetMock<DiskScanProvider>()
                .Setup(c => c.Scan(It.IsAny<Series>(), It.IsAny<String>()))
                .Returns(new List<EpisodeFile>());

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetDirectorySize(It.IsAny<String>()))
                .Returns(10);

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDropFolder(@"C:\drop\");


            //Assert
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(It.IsAny<Series>(), subFolders[0]), Times.Once());
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(It.IsAny<Series>(), subFolders[1]), Times.Never());
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(It.IsAny<Series>(), subFolders[2]), Times.Once());
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(It.IsAny<Series>(), subFolders[3]), Times.Once());
        }

        [Test]
        public void ProcessDownload_should_logError_and_return_if_size_exceeds_free_space()
        {
            var downloadName = new DirectoryInfo(@"C:\Test\Drop\30.Rock.S01E01.Pilot");

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Title = "30 Rock")
                    .With(s => s.Path = @"C:\Test\TV\30 Rock")
                    .Build();

            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.FindSeries("rock"))
                .Returns(series);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.GetDirectorySize(downloadName.FullName))
                    .Returns(10);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.FreeDiskSpace(new DirectoryInfo(series.Path)))
                    .Returns(9);

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(downloadName);


            //Assert
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(series, downloadName.FullName), Times.Never());
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void ProcessDownload_should_process_if_free_disk_space_exceeds_size()
        {
            WithLotsOfFreeDiskSpace();
            WithValidSeries();

            var downloadName = new DirectoryInfo(@"C:\Test\Drop\30.Rock.S01E01.Pilot");

            WithImportedFiles(downloadName.FullName);

            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.FindSeries("rock"))
                .Returns(fakeSeries);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.GetDirectorySize(downloadName.FullName))
                    .Returns(8);

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(downloadName);


            //Assert
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(fakeSeries, downloadName.FullName), Times.Once());
        }

        [Test]
        public void ProcessDownload_should_process_if_free_disk_space_equals_size()
        {
            var downloadName = new DirectoryInfo(@"C:\Test\Drop\30.Rock.S01E01.Pilot");

            WithImportedFiles(downloadName.FullName);
            WithValidSeries();

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.GetDirectorySize(downloadName.FullName))
                    .Returns(10);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.FreeDiskSpace(It.IsAny<DirectoryInfo>()))
                    .Returns(10);

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDownload(downloadName);


            //Assert
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(fakeSeries, downloadName.FullName), Times.Once());
        }
    }
}