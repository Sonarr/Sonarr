// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class PostDownloadProviderTest : TestBase
    {
        [TestCase("_UNPACK_The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.Unpacking, 1)]
        [TestCase("_FAILED_The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.Failed, 1)]
        [TestCase("_UNPACK_The Office (US) - S01E01E02 - Episode Title", PostDownloadStatusType.Unpacking, 2)]
        [TestCase("_FAILED_The Office (US) - S01E01E02 - Episode Title", PostDownloadStatusType.Failed, 2)]
        [TestCase("_UNPACK_The Office (US) - Season 01 - Episode Title", PostDownloadStatusType.Unpacking, 10)]
        [TestCase("_FAILED_The Office (US) - Season 01 - Episode Title", PostDownloadStatusType.Failed, 10)]
        public void ProcessFailedOrUnpackingDownload(string folderName, PostDownloadStatusType postDownloadStatus, int episodeCount)
        {
            var db = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12345)
                .With(s => s.CleanTitle = "officeus")
                .Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(episodeCount)
                .All()
                .With(c => c.SeriesId = 12345)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.PostDownloadStatus = PostDownloadStatusType.Unknown)
                .Build();

            var expectedEpisodesNumbers = fakeEpisodes.Select(e => e.EpisodeId).ToList();

            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("officeus")).Returns(fakeSeries);
            mocker.GetMock<EpisodeProvider>().Setup(s => s.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(fakeEpisodes);
            mocker.GetMock<EpisodeProvider>().Setup(s => s.GetEpisodesBySeason(12345, 1)).Returns(fakeEpisodes);
            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.SetPostDownloadStatus(expectedEpisodesNumbers, postDownloadStatus)).Verifiable();

            //Act
            mocker.Resolve<PostDownloadProvider>().ProcessFailedOrUnpackingDownload(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), folderName)), postDownloadStatus);

            //Assert
            mocker.GetMock<EpisodeProvider>().Verify(c => c.SetPostDownloadStatus(expectedEpisodesNumbers, postDownloadStatus), Times.Once());
        }

        [Test]
        public void ProcessFailedOrUnpackingDownload_Already_Existing_Time_Not_Passed()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var path = Path.Combine(Directory.GetCurrentDirectory(),
                                    "_FAILED_The Office (US) - S01E01 - Episode Provider");

            var postDownloadStatus = PostDownloadStatusType.Failed;

            var postDownloadProvider = new PostDownloadProvider();

            var model = new PostDownloadInfoModel
                            {
                                Name = path,
                                Status = postDownloadStatus,
                                Added = DateTime.Now.AddMinutes(-5)
                            };

            postDownloadProvider.Add(model);

            //Act
            mocker.Resolve<PostDownloadProvider>().ProcessFailedOrUnpackingDownload(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), path)), postDownloadStatus);

            //Assert
            mocker.VerifyAllMocks();
            postDownloadProvider.Remove(model);
        }

        [Test]
        public void ProcessFailedOrUnpackingDownload_Invalid_Episode()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var path = Path.Combine(Directory.GetCurrentDirectory(),
                                    "_FAILED_The Office (US) - S01E01 - Episode Provider");

            var postDownloadStatus = PostDownloadStatusType.Failed;

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12345)
                .With(s => s.CleanTitle = "officeus")
                .Build();

            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("officeus")).Returns(fakeSeries);
            mocker.GetMock<EpisodeProvider>().Setup(s => s.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(new List<Episode>());
            mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(It.IsAny<string>(), It.IsAny<string>()));

            //Act
            mocker.Resolve<PostDownloadProvider>().ProcessFailedOrUnpackingDownload(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), path)), postDownloadStatus);

            //Assert
            ExceptionVerification.ExcpectedWarns(1);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void ProcessDownload_InvalidSeries()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var di = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - S01E01 - Episode Title");

            var newFolder = @"C:\Test\Unsorted TV\_NzbDrone_InvalidSeries_The Office - S01E01 - Episode Title";
            Series nullSeries = null;

            //Act
            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(nullSeries);
            mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(di.FullName, newFolder));

            mocker.Resolve<PostDownloadProvider>().ProcessDownload(di);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void ProcessDownload_ParseError()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var di = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - S01E01 - Episode Title");

            var newFolder = @"C:\Test\Unsorted TV\_NzbDrone_ParseError_The Office - S01E01 - Episode Title";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "The Office")
                .Build();

            //Act
            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(fakeSeries);
            mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(di.FullName, newFolder));
            mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectorySize(di.FullName)).Returns(100.Megabytes());
            mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, di.FullName)).Returns(
                new List<EpisodeFile>());

            mocker.Resolve<PostDownloadProvider>().ProcessDownload(di);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void ProcessDownload_Unknown_Error()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var di = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - Season 01");

            var newFolder = @"C:\Test\Unsorted TV\_NzbDrone_The Office - Season 01";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "The Office")
                .Build();

            var fakeEpisodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .All()
                .With(f => f.SeriesId = fakeSeries.SeriesId)
                .Build().ToList();

            //Act
            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(fakeSeries);
            mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(di.FullName, newFolder));
            mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectorySize(di.FullName)).Returns(100.Megabytes());
            mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, di.FullName)).Returns(fakeEpisodeFiles);
            mocker.GetMock<DiskScanProvider>().Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), true)).Returns(true);

            mocker.Resolve<PostDownloadProvider>().ProcessDownload(di);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void ProcessDownload_Success()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var di = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - Season 01");

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "The Office")
                .Build();

            var fakeEpisodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .All()
                .With(f => f.SeriesId = fakeSeries.SeriesId)
                .Build().ToList();

            //Act
            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(fakeSeries);
            mocker.GetMock<DiskProvider>().Setup(s => s.DeleteFolder(di.FullName, true));
            mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectorySize(di.FullName)).Returns(1.Megabytes());
            mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, di.FullName)).Returns(fakeEpisodeFiles);
            mocker.GetMock<DiskScanProvider>().Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), true)).Returns(true);

            mocker.Resolve<PostDownloadProvider>().ProcessDownload(di);

            //Assert
            mocker.VerifyAllMocks();
        }

        [TestCase("_NzbDrone_InvalidEpisode_The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.InvalidEpisode)]
        [TestCase("_NzbDrone_InvalidSeries_The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.InvalidSeries)]
        [TestCase("_NzbDrone_ParseError_The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.ParseError)]
        [TestCase("_UNPACK_The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.Unpacking)]
        [TestCase("_FAILED_The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.Failed)]
        [TestCase("_NzbDrone_The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.Unknown)]
        [TestCase("The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.NoError)]
        public void GetPostDownloadStatusForFolder_should_return_a_proper_match(string folderName, PostDownloadStatusType expectedStatus)
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<PostDownloadProvider>().GetPostDownloadStatusForFolder(folderName);

            //Assert
            result.Should().Be(expectedStatus);
        }

        [TestCase("_NzbDrone_InvalidEpisode_", "The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.InvalidSeries)]
        [TestCase("_NzbDrone_InvalidSeries_", "The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.InvalidEpisode)]
        [TestCase("_NzbDrone_ParseError_", "The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.InvalidSeries)]
        [TestCase("_UNPACK_", "The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.InvalidEpisode)]
        [TestCase("_FAILED_", "The Office (US) - S01E01 - Title", PostDownloadStatusType.ParseError)]
        [TestCase("_NzbDrone_", "The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.ParseError)]
        public void GetNewFolderNameWithPostDownloadStatus_should_return_a_string_with_the_error_removing_existing_error(string existingErrorString, string folderName, PostDownloadStatusType postDownloadStatus)
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), existingErrorString + folderName));
            var expectedFolderName = String.Format("_NzbDrone_{0}_{1}", postDownloadStatus.ToString(), folderName);

            var expectedResult = Path.Combine(Directory.GetCurrentDirectory(), expectedFolderName);

            //Act
            var result = mocker.Resolve<PostDownloadProvider>().GetNewFolderNameWithPostDownloadStatus(di, postDownloadStatus);

            //Assert
            result.Should().Be(expectedResult);
        }

        [TestCase("The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.InvalidSeries)]
        [TestCase("The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.InvalidEpisode)]
        [TestCase("The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.ParseError)]
        public void GetNewFolderNameWithPostDownloadStatus_should_return_a_string_with_the_error(string folderName, PostDownloadStatusType postDownloadStatus)
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), folderName));
            var expectedFolderName = String.Format("_NzbDrone_{0}_{1}", postDownloadStatus.ToString(), folderName);

            var expectedResult = Path.Combine(Directory.GetCurrentDirectory(), expectedFolderName);

            //Act
            var result = mocker.Resolve<PostDownloadProvider>().GetNewFolderNameWithPostDownloadStatus(di, postDownloadStatus);

            //Assert
            result.Should().Be(expectedResult);
        }

        [TestCase("_NzbDrone_ParseError_", "The Office (US) - S01E01 - Episode Title")]
        [TestCase("", "The Office (US) - S01E01 - Episode Title")]
        public void GetNewFolderNameWithPostDownloadStatus_should_return_a_path_with_a_unknown_error(string existingError, string folderName)
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), folderName));
            var expectedFolderName = String.Format("_NzbDrone_{0}", folderName);

            var expectedResult = Path.Combine(Directory.GetCurrentDirectory(), expectedFolderName);

            //Act
            var result = mocker.Resolve<PostDownloadProvider>().GetNewFolderNameWithPostDownloadStatus(di, PostDownloadStatusType.Unknown);

            //Assert
            result.Should().Be(expectedResult);
        }

        [TestCase("_NzbDrone_ParseError_", "The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.NoError)]
        [TestCase("", "The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.NoError)]
        [TestCase("_NzbDrone_ParseError_", "The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.Processed)]
        [TestCase("", "The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.Processed)]
        public void GetNewFolderNameWithPostDownloadStatus_should_return_a_path_with_no_error(string existingError, string folderName, PostDownloadStatusType postDownloadStatus)
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), folderName));
            var expectedFolderName = folderName;

            var expectedResult = Path.Combine(Directory.GetCurrentDirectory(), expectedFolderName);

            //Act
            var result = mocker.Resolve<PostDownloadProvider>().GetNewFolderNameWithPostDownloadStatus(di, postDownloadStatus);

            //Assert
            result.Should().Be(expectedResult);
        }

        [TestCase("_NzbDrone_ParseError_The Office (US) - S01E01 - Episode Title", "The Office (US) - S01E01 - Episode Title")]
        [TestCase("_Status_The Office (US) - S01E01 - Episode Title", "The Office (US) - S01E01 - Episode Title")]
        [TestCase("The Office (US) - S01E01 - Episode Title", "The Office (US) - S01E01 - Episode Title")]
        [TestCase("_The Office (US) - S01E01 - Episode Title", "_The Office (US) - S01E01 - Episode Title")]
        public void RemoveStatus_should_remove_status_string_from_folder_name(string folderName, string cleanFolderName)
        {
            PostDownloadProvider.RemoveStatusFromFolderName(folderName).Should().Be(cleanFolderName);
        }
    }
}