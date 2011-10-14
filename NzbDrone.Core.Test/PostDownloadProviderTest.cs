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
        [TestCase("The Office (US) - S01E05 - Episode Title", PostDownloadStatusType.Unpacking, 1)]
        [TestCase("The Office (US) - S01E05 - Episode Title", PostDownloadStatusType.Failed, 1)]
        [TestCase("The Office (US) - S01E05E06 - Episode Title", PostDownloadStatusType.Unpacking, 2)]
        [TestCase("The Office (US) - S01E05E06 - Episode Title", PostDownloadStatusType.Failed, 2)]
        [TestCase("The Office (US) - Season 01 - Episode Title", PostDownloadStatusType.Unpacking, 10)]
        [TestCase("The Office (US) - Season 01 - Episode Title", PostDownloadStatusType.Failed, 10)]
        public void SetPostDownloadStatus(string folderName, PostDownloadStatusType postDownloadStatus, int episodeCount)
        {
            var db = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12345)
                .With(s => s.CleanTitle = "officeus")
                .Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10)
                .WhereAll()
                .Have(c => c.SeriesId = 12345)
                .Have(c => c.SeasonNumber = 1)
                .Have(c => c.PostDownloadStatus = PostDownloadStatusType.Unknown)
                .Build();

            db.Insert(fakeSeries);
            db.InsertMany(fakeEpisodes);

            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("officeus")).Returns(fakeSeries);

            //Act
            //mocker.Resolve<EpisodeProvider>().SetPostDownloadStatus(folderName, postDownloadStatus);

            //Assert
            var result = db.Fetch<Episode>();
            result.Where(e => e.PostDownloadStatus == postDownloadStatus).Count().Should().Be(episodeCount);
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