using System;
using System.Collections.Generic;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using PetaPoco;

namespace NzbDrone.Core.Test
{
    // ReSharper disable InconsistentNaming
    public class DiskScanProviderTest : TestBase
    {
        [Test]
        public void import_new_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"WEEDS.S03E01.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";
            const int seasonNumber = 3;
            const int episodeNumner = 1;
            const int size = 12345;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(c => c.SeriesId = fakeSeries.SeriesId)
                .With(c => c.SeasonNumber = seasonNumber)
                .Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            var database = mocker.GetMock<IDatabase>(MockBehavior.Strict);
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1).Verifiable();


            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns(fakeEpisode);

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNotNull(result);
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Once());
            mocker.VerifyAllMocks();

            result.SeasonNumber.Should().Be(fakeEpisode.SeasonNumber);

            Assert.AreEqual(fakeEpisode.SeriesId, result.SeriesId);
            Assert.AreEqual(QualityTypes.DVD, result.Quality);
            Assert.AreEqual(Parser.NormalizePath(fileName), result.Path);
            Assert.AreEqual(size, result.Size);
            Assert.AreEqual(false, result.Proper);
            Assert.AreNotEqual(new DateTime(), result.DateAdded);
        }

        [TestCase(QualityTypes.SDTV, true)]
        [TestCase(QualityTypes.HDTV, true)]
        public void import_new_file_with_better_quality(QualityTypes currentFileQuality, bool currentFileProper)
        {
            const string fileName = @"WEEDS.S03E01.DUAL.1080p.-HELLYWOOD.mkv";
            const int seasonNumber = 3;
            const int episodeNumner = 1;
            const int size = 12345;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(c => c.SeriesId = fakeSeries.SeriesId)
                .With(c => c.SeasonNumber = seasonNumber)
                .With(e => e.EpisodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(g => g.Quality = (QualityTypes)currentFileQuality)
                                               .And(g => g.Proper = currentFileProper).Build()
                        )
                        .Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            var database = mocker.GetMock<IDatabase>(MockBehavior.Strict);
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1).Verifiable();


            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns(fakeEpisode);

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNotNull(result);
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Once());
            mocker.VerifyAllMocks();

            result.SeasonNumber.Should().Be(fakeEpisode.SeasonNumber);

            Assert.AreEqual(fakeEpisode.SeriesId, result.SeriesId);
            Assert.AreEqual(QualityTypes.SDTV, result.Quality);
            Assert.AreEqual(Parser.NormalizePath(fileName), result.Path);
            Assert.AreEqual(size, result.Size);
            Assert.AreEqual(false, result.Proper);
            Assert.AreNotEqual(new DateTime(), result.DateAdded);
        }


        [TestCase("WEEDS.S03E01.DUAL.DVD.XviD.AC3.-HELLYWOOD.avi")]
        [TestCase("WEEDS.S03E01.DUAL.SDTV.XviD.AC3.-HELLYWOOD.avi")]
        public void import_new_file_skip_if_episode_has_same_or_better_quality(string fileName)
        {
            const int seasonNumber = 3;
            const int episodeNumner = 1;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(c => c.SeriesId = fakeSeries.SeriesId)
                .With(c => c.SeasonNumber = seasonNumber)
                .With(c => c.EpisodeFile = Builder<EpisodeFile>.CreateNew()
                        .With(e => e.Quality = QualityTypes.Bluray720p).Build()
                     )
                .Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            var database = mocker.GetMock<IDatabase>(MockBehavior.Strict);
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns(fakeEpisode);

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            result.Should().BeNull();
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Never());
            mocker.VerifyAllMocks();
        }


        [Test]
        public void import_new_daily_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"2011.01.10 - Denis Leary - HD TV.mkv";
            var airDate = new DateTime(2011, 01, 10);
            const int size = 12345;
            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = fakeSeries.SeriesId).Build();

            //Mocks
            var mocker = new AutoMoqer();

            var database = mocker.GetMock<IDatabase>(MockBehavior.Strict);
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, airDate)).Returns(fakeEpisode).
                Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNotNull(result);
            mocker.GetMock<IDatabase>().VerifyAll();
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Once());
            mocker.GetMock<EpisodeProvider>().VerifyAll();
            mocker.GetMock<DiskProvider>().VerifyAll();

            //Currently can't verify this since the list of episodes are loaded
            //Dynamically by SubSonic
            //Assert.AreEqual(fakeEpisode, result.EpisodeNumbers[0]);

            Assert.AreEqual(fakeEpisode.SeriesId, result.SeriesId);
            Assert.AreEqual(QualityTypes.HDTV, result.Quality);
            Assert.AreEqual(Parser.NormalizePath(fileName), result.Path);
            Assert.AreEqual(size, result.Size);
            Assert.AreEqual(false, result.Proper);
            Assert.AreNotEqual(new DateTime(), result.DateAdded);
        }

        [Test]
        public void import_existing_season_file_should_skip()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"WEEDS.S03E01.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IDatabase>(MockBehavior.Strict)
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(true).Verifiable();

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            mocker.VerifyAllMocks();
            result.Should().BeNull();
        }

        [Test]
        public void import_unparsable_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"WEEDS.avi";
            const int size = 12345;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IDatabase>(MockBehavior.Strict)
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            mocker.VerifyAllMocks();
            Assert.IsNull(result);
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void import_sample_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"2011.01.10 - Denis Leary - sample - HD TV.mkv";
            var airDate = new DateTime(2011, 01, 10);
            const int size = 12345;
            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = fakeSeries.SeriesId).Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IDatabase>()
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>())).Returns(false).Verifiable();
            mocker.GetMock<IDatabase>()
                .Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(0).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, airDate)).Returns(fakeEpisode).
                Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();


            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void import_existing_file()
        {
            const string fileName = "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";

            var fakeSeries = Builder<Series>.CreateNew().Build();

            var mocker = new AutoMoqer();
            mocker.GetMock<IDatabase>(MockBehavior.Strict)
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(true).Verifiable();

            mocker.GetMock<EpisodeProvider>(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            result.Should().BeNull();
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Never());
            mocker.VerifyAllMocks();
        }

        [Test]
        public void import_file_with_no_episode_in_db_should_return_null()
        {

            //Constants
            const string fileName = "WEEDS.S03E01.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";
            const int seasonNumber = 3;
            const int episodeNumner = 01;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();
            mocker.GetMock<IDatabase>(MockBehavior.Strict)
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();

            mocker.GetMock<EpisodeProvider>(MockBehavior.Strict)
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns<Episode>(null).
                Verifiable();

            mocker.GetMock<DiskProvider>(MockBehavior.Strict)
                .Setup(e => e.GetSize(fileName)).Returns(90000000000);


            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            mocker.VerifyAllMocks();
            result.Should().BeNull();
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Never());
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void scan_series_should_update_last_scan_date()
        {

            var mocker = new AutoMoqer();
            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.UpdateSeries(It.Is<Series>(s => s.LastDiskSync != null))).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeBySeries(It.IsAny<long>()))
                .Returns(new List<Episode> { new Episode() });


            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetSeriesFiles(It.IsAny<int>()))
                .Returns(new List<EpisodeFile>());

            mocker.Resolve<DiskScanProvider>().Scan(new Series());

            mocker.VerifyAllMocks();

        }
    }
}
