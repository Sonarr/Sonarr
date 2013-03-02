// ReSharper disable RedundantUsingDirective

using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFileTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class MediaFileServiceTest : ObjectDbTest
    {
        [Test]
        public void get_series_files()
        {
            var firstSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Quality = Quality.SDTV)
                .With(s => s.SeriesId = 12).Build();

            var secondSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Quality = Quality.SDTV)
                .With(s => s.SeriesId = 20).Build();




            Db.InsertMany(firstSeriesFiles);
            Db.InsertMany(secondSeriesFiles);

            var result = Mocker.Resolve<IMediaFileService>().GetFilesBySeries(12);


            result.Should().HaveSameCount(firstSeriesFiles);
        }

        [Test]
        public void get_season_files()
        {
            var firstSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Quality = Quality.SDTV)
                .With(s => s.SeriesId = 12)
                .With(s => s.SeasonNumber = 1)
                .Build();

            var secondSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Quality = Quality.SDTV)
                .With(s => s.SeriesId = 12)
                .With(s => s.SeasonNumber = 2)
                .Build();



            Db.InsertMany(firstSeriesFiles);
            Db.InsertMany(secondSeriesFiles);

            var result = Mocker.Resolve<IMediaFileService>().GetFilesBySeason(12, 1);

            result.Should().HaveSameCount(firstSeriesFiles);
        }

        [Test]
        public void Scan_series_should_skip_series_with_no_episodes()
        {
            WithStrictMocker();

            Mocker.GetMock<IEpisodeService>()
                .Setup(c => c.GetEpisodeBySeries(12))
                .Returns(new List<Episode>());


            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<string>()))
                .Returns(true);

            var series = Builder<Series>.CreateNew()
                .With(s => s.Id = 12).Build();

            //Act
            Mocker.Resolve<DiskScanProvider>().Scan(series);

            //Assert
            Mocker.VerifyAllMocks();

        }

        [Test]
        [TestCase("Law & Order: Criminal Intent - S10E07 - Icarus [HDTV-720p]", "Law & Order- Criminal Intent - S10E07 - Icarus [HDTV-720p]")]
        public void CleanFileName(string name, string expectedName)
        {
            //Act
            var result = MediaFileService.CleanFilename(name);

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


            Mocker.GetMock<IConfigService>().Setup(e => e.SortingSeasonFolderFormat).Returns(seasonFolderFormat);

            //Act
            var result = Mocker.Resolve<IMediaFileService>().CalculateFilePath(fakeSeries, 1, filename, ".mkv");

            //Assert
            Assert.AreEqual(expectedPath, result.FullName);
        }

        [Test]
        public void DeleteEpisodeFile()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>
                .CreateListOfSize(10)
                .All()
                .With(c => c.Quality = Quality.SDTV)
                .Build();


            Db.InsertMany(episodeFiles);

            //Act
            Mocker.Resolve<IMediaFileService>().Delete(1);

           
        }

        [Test]
        public void GetFileByPath_should_return_null_if_file_does_not_exist_in_database()
        {
            //Setup

            //Act
            var result = Mocker.Resolve<IMediaFileService>().GetFileByPath(@"C:\Test\EpisodeFile.avi");

            //Resolve
            result.Should().BeNull();
        }

        [Test]
        public void GetFileByPath_should_return_EpisodeFile_if_file_exists_in_database()
        {
            var path = @"C:\Test\EpisodeFile.avi";

            //Setup
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                    .With(c => c.Quality = Quality.SDTV)
                    .With(f => f.Path = path.NormalizePath())
                    .Build();

            var episodeFileId = Convert.ToInt32(Db.Insert(episodeFile));

            //Act
            var result = Mocker.Resolve<IMediaFileService>().GetFileByPath(path);

            //Resolve
            result.Should().NotBeNull();
            result.Path.Should().Be(path.NormalizePath());
            result.Id.Should().Be(episodeFileId);
        }
    }
}