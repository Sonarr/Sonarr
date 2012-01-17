// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class MediaFileProviderTest : CoreTest
    {
        [Test]
        public void get_series_files()
        {
            var firstSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .All().With(s => s.SeriesId = 12).Build();

            var secondSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .All().With(s => s.SeriesId = 20).Build();



            var database = TestDbHelper.GetEmptyDatabase(true);


            database.InsertMany(firstSeriesFiles);
            database.InsertMany(secondSeriesFiles);

            Mocker.SetConstant(database);

            var result = Mocker.Resolve<MediaFileProvider>().GetSeriesFiles(12);


            result.Should().HaveSameCount(firstSeriesFiles);
        }

        [Test]
        public void get_season_files()
        {
            var firstSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .All()
                .With(s => s.SeriesId = 12)
                .With(s => s.SeasonNumber = 1)
                .Build();

            var secondSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .All()
                .With(s => s.SeriesId = 12)
                .With(s => s.SeasonNumber = 2)
                .Build();



            var database = TestDbHelper.GetEmptyDatabase(true);

            database.InsertMany(firstSeriesFiles);
            database.InsertMany(secondSeriesFiles);

            Mocker.SetConstant(database);

            var result = Mocker.Resolve<MediaFileProvider>().GetSeasonFiles(12, 1);

            result.Should().HaveSameCount(firstSeriesFiles);
        }

        [Test]
        public void Scan_series_should_skip_series_with_no_episodes()
        {
            WithStrictMocker();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeBySeries(12))
                .Returns(new List<Episode>());

            Mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.CleanUpDatabase());


            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<string>()))
                .Returns(true);

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12).Build();

            //Act
            Mocker.Resolve<DiskScanProvider>().Scan(series);

            //Assert
            Mocker.VerifyAllMocks();

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


            Mocker.GetMock<ConfigProvider>().Setup(e => e.SortingSeasonFolderFormat).Returns(seasonFolderFormat);

            //Act
            var result = Mocker.Resolve<MediaFileProvider>().CalculateFilePath(fakeSeries, 1, filename, ".mkv");

            //Assert
            Assert.AreEqual(expectedPath, result.FullName);
        }

        [Test]
        public void DeleteEpisodeFile()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10).Build();


            var database = TestDbHelper.GetEmptyDatabase(true);
            Mocker.SetConstant(database);
            database.InsertMany(episodeFiles);

            //Act
            Mocker.Resolve<MediaFileProvider>().Delete(1);
            var result = database.Fetch<EpisodeFile>();

            //Assert
            result.Should().HaveCount(9);
            result.Should().NotContain(e => e.EpisodeFileId == 1);
        }
    }
}