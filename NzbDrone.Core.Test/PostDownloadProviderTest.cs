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
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using PetaPoco;
using TvdbLib.Data;

namespace NzbDrone.Core.Test
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
                .WhereAll()
                .Have(c => c.SeriesId = 12345)
                .Have(c => c.SeasonNumber = 1)
                .Have(c => c.PostDownloadStatus = PostDownloadStatusType.Unknown)
                .Build();

            var expectedEpisodesNumbers = fakeEpisodes.Select(e => e.EpisodeId);

            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("officeus")).Returns(fakeSeries);
            mocker.GetMock<EpisodeProvider>().Setup(s => s.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(fakeEpisodes);
            mocker.GetMock<EpisodeProvider>().Setup(s => s.GetEpisodesBySeason(12345, 1)).Returns(fakeEpisodes);
            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.SetPostDownloadStatus(expectedEpisodesNumbers, postDownloadStatus)).Verifiable();

            //Act
            mocker.Resolve<PostDownloadProvider>().ProcessFailedOrUnpackingDownload(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), folderName)),postDownloadStatus);

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
            postDownloadProvider.Add(new PostDownloadInfoModel
                                         {
                                             Name = path,
                                             Status = postDownloadStatus,
                                             Added = DateTime.Now.AddMinutes(-5)
                                         });

            //Act
            mocker.Resolve<PostDownloadProvider>().ProcessFailedOrUnpackingDownload(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), path)), postDownloadStatus);

            //Assert
            mocker.VerifyAllMocks();
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

        [TestCase(PostDownloadStatusType.Unpacking, 8)]
        [TestCase(PostDownloadStatusType.Failed, 8)]
        [TestCase(PostDownloadStatusType.InvalidSeries, 24)]
        [TestCase(PostDownloadStatusType.ParseError, 21)]
        [TestCase(PostDownloadStatusType.Unknown, 10)]
        [TestCase(PostDownloadStatusType.Processed, 0)]
        public void GetPrefixLength(PostDownloadStatusType postDownloadStatus, int expected)
        {
            //Setup
            var mocker = new AutoMoqer();
            
            //Act
            var result = mocker.Resolve<PostDownloadProvider>().GetPrefixLength(postDownloadStatus);
            
            //Assert
            result.Should().Be(expected);
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
                .WhereAll()
                .Have(f => f.SeriesId = fakeSeries.SeriesId)
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
                .WhereAll()
                .Have(f => f.SeriesId = fakeSeries.SeriesId)
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
    }
}