// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using PetaPoco;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class MediaFileProviderTests : TestBase
    {
        [Test]
        public void get_series_files()
        {
            var firstSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .WhereAll().Have(s => s.SeriesId = 12).Build();

            var secondSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .WhereAll().Have(s => s.SeriesId = 20).Build();

            var mocker = new AutoMoqer();

            var database = MockLib.GetEmptyDatabase(true);


            database.InsertMany(firstSeriesFiles);
            database.InsertMany(secondSeriesFiles);

            mocker.SetConstant(database);

            var result = mocker.Resolve<MediaFileProvider>().GetSeriesFiles(12);


            result.Should().HaveSameCount(firstSeriesFiles);
        }

        [Test]
        public void get_season_files()
        {
            var firstSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .WhereAll()
                .Have(s => s.SeriesId = 12)
                .Have(s => s.SeasonNumber = 1)
                .Build();

            var secondSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .WhereAll()
                .Have(s => s.SeriesId = 12)
                .Have(s => s.SeasonNumber = 2)
                .Build();

            var mocker = new AutoMoqer();

            var database = MockLib.GetEmptyDatabase(true);

            database.InsertMany(firstSeriesFiles);
            database.InsertMany(secondSeriesFiles);

            mocker.SetConstant(database);

            var result = mocker.Resolve<MediaFileProvider>().GetSeasonFiles(12, 1);

            result.Should().HaveSameCount(firstSeriesFiles);
        }

        [Test]
        public void Scan_series_should_skip_series_with_no_episodes()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeBySeries(12))
                .Returns(new List<Episode>());

            mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.RepairLinks()).Returns(0);
            mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.DeleteOrphaned()).Returns(0);


            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12).Build();

            //Act
            mocker.Resolve<DiskScanProvider>().Scan(series);

            //Assert
            mocker.VerifyAllMocks();

        }

        [Test]
        [TestCase("Law & Order: Criminal Intent - S10E07 - Icarus [HDTV]", "Law & Order- Criminal Intent - S10E07 - Icarus [HDTV]")]
        public void CleanFileName(string name, string expectedName)
        {
            //Act
            var result = MediaFileProvider.CleanFilename(name);

            //Assert
            Assert.AreEqual(expectedName, result);
        }

        [Test]
        public void CleanEpisodesWithNonExistantFiles()
        {
            //Setup
            var episodes = Builder<Episode>.CreateListOfSize(10).Build();

            var mocker = new AutoMoqer();
            var database = MockLib.GetEmptyDatabase(true);
            mocker.SetConstant(database);
            database.InsertMany(episodes);

            //Act
            var removedLinks = mocker.Resolve<MediaFileProvider>().RepairLinks();
            var result = database.Fetch<Episode>();

            //Assert
            result.Should().HaveSameCount(episodes);
            result.Should().OnlyContain(e => e.EpisodeFileId == 0);
            removedLinks.Should().Be(10);
          }

        [Test]
        public void DeleteOrphanedEpisodeFiles()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10).Build();
            var episodes = Builder<Episode>.CreateListOfSize(5).Build();

            var mocker = new AutoMoqer();
            var database = MockLib.GetEmptyDatabase(true);
            mocker.SetConstant(database);
            database.InsertMany(episodes);
            database.InsertMany(episodeFiles);

            //Act
            mocker.Resolve<MediaFileProvider>().DeleteOrphaned();
            var result = database.Fetch<EpisodeFile>();

            //Assert
            result.Should().HaveCount(5);
            result.Should().OnlyContain(e => e.EpisodeFileId > 0);
        }

        [Test]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, true, "Season %0s", @"C:\Test\30 Rock\Season 01\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, true, "Season %s", @"C:\Test\30 Rock\Season 1\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, false, "Season %0s", @"C:\Test\30 Rock\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, false, "Season %s", @"C:\Test\30 Rock\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, true, "ReallyUglySeasonFolder %s", @"C:\Test\30 Rock\ReallyUglySeasonFolder 1\30 Rock - S01E05 - Episode Title.mkv")]
        public void CalculateFilePath_SeasonFolder_SingleNumber(string filename, int seasonNumber, bool useSeasonFolder, string seasonFolderFormat, string expectedPath)
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "30 Rock")
                .With(s => s.Path = @"C:\Test\30 Rock")
                .With(s => s.SeasonFolder = useSeasonFolder)
                .Build();

            var mocker = new AutoMoqer();
            mocker.GetMock<ConfigProvider>().Setup(e => e.SortingSeasonFolderFormat).Returns(seasonFolderFormat);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().CalculateFilePath(fakeSeries, 1, filename, ".mkv");

            //Assert
            Assert.AreEqual(expectedPath, result.FullName);
        }

        [Test]
        public void DeleteEpisodeFile()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10).Build();

            var mocker = new AutoMoqer();
            var database = MockLib.GetEmptyDatabase(true);
            mocker.SetConstant(database);
            database.InsertMany(episodeFiles);

            //Act
            mocker.Resolve<MediaFileProvider>().Delete(1);
            var result = database.Fetch<EpisodeFile>();

            //Assert
            result.Should().HaveCount(9);
            result.Should().NotContain(e => e.EpisodeFileId == 1);
        }
    }
}